"""
FastAPI-сервер для интеграции с Blazor.
Предоставляет REST API для:
- Считывания титульного листа
- Поиска атрибутов по документу
- Просмотра списка документов в базе
- Отмены длительных операций
"""

import asyncio
import json
import os
import httpx
import sys
import uuid
from contextlib import asynccontextmanager
from datetime import datetime
from typing import Optional

from fastapi import FastAPI, HTTPException, Query
from fastapi.responses import JSONResponse, ORJSONResponse
from pydantic import BaseModel
import uvicorn

from langchain_ollama import OllamaEmbeddings

from config import (
    DEFAULT_LLM_MODEL,
    DEFAULT_EMBEDDING_MODEL,
    ATTRIBUTE_SEARCH_CONFIGS,
    TITLE_ATTRIBUTES,
    TITLE_PAGES_COUNT
)
# from ollama_async import stop_ollama_generation
from pdf_loader import load_and_chunk_pdf
from vector_store import (
    get_or_create_vectordb,
    list_documents_in_db,
    add_pdf_to_existing_db
)
from search import combined_search
from llm_extractor import extract_json_from_chunks_async, extract_json_from_chunks_dynamic_async
from title_page_reader import read_title_pages, extract_title_attributes_async
from db import (
    get_pending_attributes_for_work,
    save_multiple_attributes,
    get_pending_attributes_sync,
    save_multiple_attributes_sync
)

# ============================================================
# Проверка Ollama
# ============================================================
async def check_ollama_health() -> bool:
    """
    Проверяет, запущена ли Ollama и отвечает ли её API.
    
    Returns:
        True если Ollama доступна, иначе вызывает исключение.
    """
    ollama_url = "http://localhost:11434"  # стандартный порт Ollama
    
    try:
        async with httpx.AsyncClient() as client:
            response = await client.get(f"{ollama_url}/api/tags", timeout=5.0)
            if response.status_code == 200:
                models = response.json().get("models", [])
                model_names = [m["name"] for m in models]
                print(f"[{datetime.now()}] Ollama доступна. Модели: {model_names}")
                return True
            else:
                raise ConnectionError(f"Ollama ответила с кодом {response.status_code}")
    except httpx.ConnectError:
        raise ConnectionError(
            "Ollama не запущена. Запустите её командой 'ollama serve' "
            "или через приложение Ollama. Скачать: https://ollama.com/download"
        )
    except httpx.TimeoutException:
        raise ConnectionError(
            "Ollama не отвечает (таймаут). Проверьте, что она запущена."
        )


# ============================================================
# Lifespan
# ============================================================
@asynccontextmanager
async def lifespan(app: FastAPI):
    global embeddings
    
    # 1. Проверяем Ollama
    print(f"[{datetime.now()}] Проверка Ollama...")
    try:
        await check_ollama_health()
    except ConnectionError as e:
        print(f"[{datetime.now()}] КРИТИЧЕСКАЯ ОШИБКА: {e}")
        print("[{datetime.now()}] Сервер не может работать без Ollama. Остановка.")
        import sys
        sys.exit(1)
    
    # 2. Проверяем, что нужные модели скачаны
    print(f"[{datetime.now()}] Проверка моделей...")
    async with httpx.AsyncClient() as client:
        response = await client.get("http://localhost:11434/api/tags")
        models = [m["name"] for m in response.json().get("models", [])]
        
        required_models = [DEFAULT_LLM_MODEL, DEFAULT_EMBEDDING_MODEL]
        for model in required_models:
            # Модель может быть с тегом (llama3.2:latest)
            found = any(m.startswith(model.split(":")[0]) for m in models)
            if not found:
                print(f"[{datetime.now()}] ВНИМАНИЕ: Модель '{model}' не найдена.")
                print(f"[{datetime.now()}] Скачайте её: ollama pull {model}")
    
    # 3. Инициализируем эмбеддинги
    print(f"[{datetime.now()}] Инициализация модели эмбеддингов ({DEFAULT_EMBEDDING_MODEL})...")
    embeddings = OllamaEmbeddings(model=DEFAULT_EMBEDDING_MODEL)
    print(f"[{datetime.now()}] Сервер готов.")
    
    yield  # Приложение работает
    
    for task_id, task in active_tasks.items():
        if not task.done():
            task.cancel()
    # await stop_ollama_generation()
    print(f"[{datetime.now()}] Сервер остановлен.")


app = FastAPI(
    title="Blazor AI Extractor API",
    version="1.0.0",
    lifespan=lifespan,
    default_response_class=ORJSONResponse
)

# Хранилище активных задач (для отмены)
active_tasks: dict[str, asyncio.Task] = {}

# Модели эмбеддингов (инициализируются при старте)
embeddings = None
cancel_events: dict[str, asyncio.Event] = {}

# ============================================================
# Модели запросов/ответов
# ============================================================
class SearchRequest(BaseModel):
    file_path: str  # путь к файлу в базе
    model_ai: str = DEFAULT_LLM_MODEL


class TitleExtractRequest(BaseModel):
    file_path: str
    model_ai: str = DEFAULT_LLM_MODEL
    max_pages: int = TITLE_PAGES_COUNT


class TaskStatusResponse(BaseModel):
    task_id: str
    status: str  # "running", "completed", "cancelled", "error"
    result: Optional[dict] = None
    error: Optional[str] = None


class ProcessWorkRequest(BaseModel):
    file_path: str
    work_id: int
    model_ai: str = DEFAULT_LLM_MODEL


# ============================================================
# Эндпоинты
# ============================================================

@app.get("/ai-extract/health")
async def health_check():
    """
    Проверка работоспособности всего стека.
    GET /api/health
    
    Ответ:
    {
        "status": "ok" | "degraded" | "error",
        "ollama": {"running": true, "models": [...]},
        "chroma_db": true,
        "timestamp": "..."
    }
    """
    result = {
        "status": "ok",
        "ollama": {"running": False, "models": [], "error": None},
        "chroma_db": False,
        "timestamp": datetime.now().isoformat()
    }
    
    # Проверка Ollama
    try:
        async with httpx.AsyncClient() as client:
            response = await client.get("http://localhost:11434/api/tags", timeout=3.0)
            if response.status_code == 200:
                result["ollama"]["running"] = True
                result["ollama"]["models"] = [
                    m["name"] for m in response.json().get("models", [])
                ]
    except httpx.ConnectError:
        result["ollama"]["error"] = "Ollama не запущена"
        result["status"] = "error"
    except Exception as e:
        result["ollama"]["error"] = str(e)
        result["status"] = "degraded"
    
    # Проверка ChromaDB
    try:
        vectordb = get_or_create_vectordb(embeddings=embeddings)
        result["chroma_db"] = vectordb is not None
    except Exception as e:
        result["chroma_db"] = False
    
    return result


# --- Документы ---

@app.get("/ai-extract/documents")
async def get_documents():
    """Возвращает список всех документов в базе."""
    vectordb = get_or_create_vectordb(embeddings=embeddings)
    if vectordb is None:
        return JSONResponse(content={"documents": [], "error": "База не найдена"})

    docs = list_documents_in_db(vectordb)
    return {"documents": docs}


@app.post("/ai-extract/documents/index/{file_path:path}")
async def index_document(
    file_path: str,
    force_rebuild: bool = Query(False)
):
    """
    Индексирует существующий на диске PDF-файл в базу Chroma.

    POST /ai-extract/documents/index/путь/к/файлу.pdf
    Параметры:
        force_rebuild: если true — пересоздаёт базу заново (для первого файла).

    Ответ:
        { "file_path": "...", "indexed": true, "chunks_added": 45, "total_chunks": 45, "error": null }
    """
    # Проверяем, что файл существует
    if not os.path.exists(file_path):
        return {"error": f"Файл не найден: {file_path}"}

    if not file_path.lower().endswith('.pdf'):
        return {"error": "Только PDF-файлы"}

    print(f"[{datetime.now()}] Индексация файла: {file_path}")

    try:
        # Если force_rebuild — создаём базу с нуля
        if force_rebuild:
            chunks, metadatas = load_and_chunk_pdf(file_path, short_path=file_path)
            vectordb = get_or_create_vectordb(
                chunks=chunks,
                metadatas=metadatas,
                embeddings=embeddings,
                force_rebuild=True
            )
            if vectordb is None:
                return {"error": "Ошибка создания базы"}
            return {
                "file_path": file_path,
                "indexed": True,
                "chunks_added": len(chunks),
                "total_chunks": vectordb._collection.count(),
                "base_recreated": True
            }

        # Иначе добавляем в существующую
        vectordb = add_pdf_to_existing_db(file_path, embeddings, short_path=file_path)
        if vectordb is None:
            temp_vectordb = get_or_create_vectordb(embeddings=embeddings)
            return {
                "file_path": file_path,
                "indexed": False,
                "message": "Документ уже в базе (пропущено)",
                "total_chunks": temp_vectordb._collection.count() if temp_vectordb is not None else 0
            }

        return {
            "file_path": file_path,
            "indexed": True,
            "message": "Документ добавлен в базу",
            "total_chunks": vectordb._collection.count()
        }
    except Exception as e:
        return {"error": str(e)}

@app.delete("/ai-extract/documents/remove/{file_path:path}")
async def delete_document(file_path: str):
    """Удаляет документ из базы (но не удаляет файл с диска)."""
    vectordb = get_or_create_vectordb(embeddings=embeddings)
    if vectordb is None:
        return {"error": "База не найдена"}

    available_docs = list_documents_in_db(vectordb)
    if file_path not in available_docs:
        return {"error": f"Документ не найден в базе. Доступные: {available_docs}"}

    # Удаляем по фильтру full_path
    before_count = vectordb._collection.count()
    vectordb._collection.delete(where={"full_path": file_path})
    after_count = vectordb._collection.count()

    print(f"[{datetime.now()}] Удалён из базы: {file_path} (чанков удалено: {before_count - after_count})")

    return {
        "file_path": file_path,
        "removed": True,
        "chunks_removed": before_count - after_count,
        "remaining_chunks": after_count
    }


# --- Титульный лист (асинхронно с отменой) ---

@app.post("/ai-extract/extract-title/start")
async def start_title_extraction(request: TitleExtractRequest):
    """
    Запускает извлечение атрибутов титульного листа.
    Возвращает task_id для отслеживания и отмены.
    """
    task_id = str(uuid.uuid4())
    cancel_events[task_id] = asyncio.Event()

    if not os.path.exists(request.file_path):
        return {
            "status": "error",
            "error": f"Файл не найден: {request.file_path}"
        }

    # Создаём асинхронную задачу
    task = asyncio.create_task(
        _run_title_extraction(
            task_id, request.file_path, request.model_ai,
            request.max_pages, cancel_events[task_id]
        )
    )
    active_tasks[task_id] = task

    return {
        "task_id": task_id,
        "status": "running",
        "message": "Извлечение титульного листа запущено"
    }


@app.get("/ai-extract/extract-title/status/{task_id}")
async def get_title_extraction_status(task_id: str):
    """Проверяет статус задачи извлечения титульника."""
    return _get_task_status(task_id, "title")


@app.post("/ai-extract/extract-title/cancel/{task_id}")
async def cancel_title_extraction(task_id: str):
    """Отменяет задачу извлечения титульника."""
    return _cancel_task(task_id)



# --- Поиск атрибутов (асинхронно с отменой) ---

@app.post("/ai-extract/search-attributes/start")
async def start_search(request: SearchRequest):
    """
    Запускает поиск атрибутов по документу.
    Возвращает task_id для отслеживания и отмены.
    """
    task_id = str(uuid.uuid4())
    cancel_events[task_id] = asyncio.Event()

    vectordb = get_or_create_vectordb(embeddings=embeddings)
    if vectordb is None:
        return {"status": "error", "error": "База не найдена"}

    available_docs = list_documents_in_db(vectordb)
    if request.file_path not in available_docs:
        return {"status": "error", "error": f"Документ не найден в базе. Доступные: {available_docs}"}

    task = asyncio.create_task(
        _run_search(task_id, request.file_path, request.model_ai, cancel_events[task_id])
    )
    active_tasks[task_id] = task

    return {
        "task_id": task_id,
        "status": "running",
        "message": "Поиск атрибутов запущен"
    }


@app.get("/ai-extract/search-attributes/status/{task_id}")
async def get_search_status(task_id: str):
    """Проверяет статус задачи поиска."""
    return _get_task_status(task_id, "search")


@app.post("/ai-extract/search-attributes/cancel/{task_id}")
async def cancel_search(task_id: str):
    """Отменяет задачу поиска."""
    return _cancel_task(task_id)


@app.post("/api/process-work/start")
async def start_process_work(request: ProcessWorkRequest):
    """
    Главный эндпоинт для обработки работы.
    
    1. Загружает из БД все атрибуты для работы с пометкой 'Ожидание поиска...'
    2. Для каждого атрибута выполняет combined_search + LLM
    3. Сохраняет найденные значения в данные_по_атриб
    
    POST /api/process-work/start
    Body: { "file_path": "...", "work_id": 123 }
    """
    
    vectordb = get_or_create_vectordb(embeddings=embeddings)
    if vectordb is None:
        return {"status": "error", "error": "База не найдена"}

    available_docs = list_documents_in_db(vectordb)
    if request.file_path not in available_docs:
        return {"status": "error", "error": f"Документ не найден в базе. Доступные: {available_docs}"}

    # Загружаем атрибуты из БД
    print(f"\n[{datetime.now()}] Загрузка атрибутов для работы #{request.work_id}...")
    pending_attrs = await get_pending_attributes_for_work(request.work_id)

    if not pending_attrs:
        return {
            "status": "completed",
            "message": f"Нет атрибутов со статусом 'Ожидание поиска...' для работы #{request.work_id}",
            "attributes_processed": 0
        }

    print(f"  Найдено {len(pending_attrs)} атрибутов для обработки:")
    for attr in pending_attrs:
        print(f"    - {attr['название']} (id_атрибута={attr['id_атрибута']}, id_данных={attr['id_данных']})")

    # Запускаем фоновую задачу
    task_id = str(uuid.uuid4())
    cancel_events[task_id] = asyncio.Event()
    task = asyncio.create_task(
        _run_process_work(
            task_id,
            request.file_path,
            request.work_id,
            request.model_ai,
            pending_attrs,
            cancel_events[task_id]
        )
    )
    active_tasks[task_id] = task

    return {
        "task_id": task_id,
        "status": "running",
        "attributes_to_process": len(pending_attrs),
        "attribute_names": [a["название"] for a in pending_attrs]
    }


@app.get("/api/process-work/status/{task_id}")
async def get_process_work_status(task_id: str):
    """Статус обработки работы."""
    return _get_task_status(task_id, "process_work")


@app.post("/api/process-work/cancel/{task_id}")
async def cancel_process_work(task_id: str):
    """Отмена обработки работы."""
    return _cancel_task(task_id)

# ============================================================
# Фоновые задачи
# ============================================================

async def _run_title_extraction(task_id: str, file_path: str, model_ai: str, max_pages: int, cancel_event: asyncio.Event) -> dict:
    """Асинхронная обёртка для извлечения титульного листа."""
    try:
        loop = asyncio.get_event_loop()
        title_text = await loop.run_in_executor(
            None, read_title_pages, file_path, max_pages
        )

        if not title_text.strip():
            return {"error": "Нет текста", "title": {}}

        # Извлекаем атрибуты (асинхронно, с отменой)
        attrs = await extract_title_attributes_async(
            model_ai, title_text, TITLE_ATTRIBUTES, cancel_event
        )

        return {"raw_text": title_text[:3000], "title": attrs or {}}

    except asyncio.CancelledError:
        print(f"[{datetime.now()}] Задача {task_id} ОТМЕНЕНА (титульник)")
        raise


async def _run_search(task_id: str, file_path: str, model_ai: str, cancel_event: asyncio.Event) -> dict:
    """Асинхронная обёртка для поиска атрибутов."""
    try:
        # Поиск чанков (синхронно, но быстро)
        loop = asyncio.get_event_loop()
        all_docs = await loop.run_in_executor(
            None, _sync_search_chunks, file_path
        )

        if not all_docs:
            return {"error": "Ничего не найдено", "attributes": {}}

        text = "\n\n---\n\n".join(
            f"[КУСОК {i+1}]\n{doc}" for i, doc in enumerate(all_docs)
        )

        # Извлекаем атрибуты (асинхронно, с отменой)
        attrs = await extract_json_from_chunks_async(
            model_ai, text, ATTRIBUTE_SEARCH_CONFIGS, cancel_event
        )

        return {"chunks_found": len(all_docs), "attributes": attrs or {}}

    except asyncio.CancelledError:
        print(f"[{datetime.now()}] Задача {task_id} ОТМЕНЕНА (поиск)")
        raise


def _sync_search_chunks(file_path: str) -> list[str]:
    """Синхронный поиск чанков (быстрая операция)."""
    vectordb = get_or_create_vectordb(embeddings=embeddings)
    if vectordb is None: return []
    all_docs, seen = [], set()

    for config in ATTRIBUTE_SEARCH_CONFIGS.values():
        for content in combined_search(vectordb, config["query"], config["keywords"], file_path):
            if content not in seen:
                seen.add(content)
                all_docs.append(content)

    return all_docs


async def _run_process_work(
    task_id: str,
    file_path: str,
    work_id: int,
    model_ai: str,
    pending_attrs: list[dict],
    cancel_event: asyncio.Event
) -> dict:
    """Асинхронная обёртка для поиска атрибутов."""
    try:
        # Поиск чанков (синхронно, но быстро)
        loop = asyncio.get_event_loop()
        all_docs = await loop.run_in_executor(
            None, _sync_search_chunks, file_path
        )

        if not all_docs:
            return {"error": "Ничего не найдено", "attributes": {}}

        text = "\n\n---\n\n".join(
            f"[КУСОК {i+1}]\n{doc}" for i, doc in enumerate(all_docs)
        )

        # Извлекаем атрибуты (асинхронно, с отменой)
        attrs = await extract_json_from_chunks_dynamic_async(
            model_ai, text, pending_attrs, cancel_event
        )
        
        updates = []

        if attrs:
            for attr_config in pending_attrs:
                attr_name = attr_config["название"]       # например "среда разработки"
                data_id = attr_config["id_данных"]        # например 42
                
                # Ищем значение в ответе LLM по названию атрибута
                value = attrs.get(attr_name)
                
                # Если не нашли по точному названию — пробуем найти
                # похожий ключ (на случай, если LLM изменила регистр или пробелы)
                if value is None:
                    for key, val in attrs.items():
                        if key.strip().lower() == attr_name.strip().lower():
                            value = val
                            break
                
                # Если всё ещё None — ставим Н/Д
                if value is None or str(value).strip() == "":
                    value = "Н/Д"
                
                updates.append({
                    "id_данных": data_id,
                    "данные": str(value)
                })
        else:
            # LLM вернула None — всё в Н/Д
            for attr_config in pending_attrs:
                updates.append({
                    "id_данных": attr_config["id_данных"],
                    "данные": "Н/Д (ошибка LLM)"
                })

        # === ЭТАП 3: Сохранение в БД ===
        print(f"\n  Сохранение {len(updates)} значений в БД...")
        save_multiple_attributes_sync(updates)

        # Формируем ответ
        results_summary = []
        for attr in pending_attrs:
            updated = next((u for u in updates if u["id_данных"] == attr["id_данных"]), None)
            results_summary.append({
                "id_данных": attr["id_данных"],
                "атрибут": attr["название"],
                "значение": updated["данные"] if updated else "Н/Д"
            })
        
        return {
            "work_id": work_id,
            "attributes_processed": len(pending_attrs),
            "results": results_summary
        }
    except asyncio.CancelledError:
        print(f"[{datetime.now()}] Задача {task_id} ОТМЕНЕНА (поиск)")
        raise


def _get_task_status(task_id: str, prefix: str):
    task = active_tasks.get(task_id)
    if task is None:
        result_file = os.path.join("./tasks", f"{prefix}_{task_id}.json")
        if os.path.exists(result_file):
            with open(result_file, "r", encoding="utf-8") as f:
                return {"task_id": task_id, "status": "completed", "result": json.load(f)}
        return {"status": "error", "error": "Задача не найдена"}

    if task.done():
        try:
            result = task.result()
            _save_task_result(task_id, prefix, result)
            del active_tasks[task_id]
            return {"task_id": task_id, "status": "completed", "result": result}
        except asyncio.CancelledError:
            _save_task_result(task_id, prefix, {
                "task_id": task_id,
                "status": "cancelled",
                "result": None
            })
            del active_tasks[task_id]
            return {"task_id": task_id, "status": "cancelled", "result": None}
        except Exception as e:
            _save_task_result(task_id, prefix, {
                "task_id": task_id,
                "status": "error",
                "error": str(e)
            })
            del active_tasks[task_id]
            return {"task_id": task_id, "status": "error", "error": str(e)}

    return {"task_id": task_id, "status": "running"}


def _cancel_task(task_id: str):
    task = active_tasks.get(task_id)
    if task is None:
        return {"error": "Задача не найдена или уже завершена"}

    if task_id in cancel_events:
        cancel_events[task_id].set()

    task.cancel()
    return {"task_id": task_id, "status": "cancelled"}

# ============================================================
# Хранилище результатов
# ============================================================
def _save_task_result(task_id: str, prefix: str, result: dict):
    """Сохраняет результат задачи на диск."""
    os.makedirs("./tasks", exist_ok=True)
    file_path = os.path.join("./tasks", f"{prefix}_{task_id}.json")
    with open(file_path, "w", encoding="utf-8") as f:
        json.dump(result, f, ensure_ascii=False, indent=2)


# ============================================================
# Запуск сервера
# ============================================================
if __name__ == "__main__":
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8000
    print(f"Запуск сервера на порту {port}...")
    uvicorn.run(app, host="0.0.0.0", port=port)