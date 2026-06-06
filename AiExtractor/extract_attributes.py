"""
Главный модуль: CLI-интерфейс и оркестрация всех компонентов.
"""

import sys
import os
from datetime import datetime
from langchain_ollama import OllamaEmbeddings

from config import (
    DEFAULT_LLM_MODEL,
    DEFAULT_EMBEDDING_MODEL,
    ATTRIBUTE_SEARCH_CONFIGS,
    TITLE_ATTRIBUTES,
    TITLE_PAGES_COUNT
)
from pdf_loader import load_and_chunk_pdf
from vector_store import (
    get_or_create_vectordb,
    list_documents_in_db,
    add_pdf_to_existing_db
)
from search import combined_search
from llm_extractor import extract_json_from_chunks
from title_page_reader import read_title_pages, extract_title_attributes_with_llm


def print_usage():
    """Выводит инструкцию по использованию."""
    print("Использование:")
    print("  Индексация:       python main.py путь.pdf --index [модель_ИИ] [модель_эмбеддингов]")
    print("  Добавление:       python main.py путь.pdf --add [модель_ИИ] [модель_эмбеддингов]")
    print("  Поиск:            python main.py путь.pdf --search [модель_ИИ] [модель_эмбеддингов]")
    print("  Титульный лист:   python main.py путь.pdf --title  [модель_ИИ] [модель_эмбеддингов]")
    print()
    print("  По умолчанию:     модель_ИИ = llama3.2, модель_эмбеддингов = all-minilm")


def parse_args():
    """Разбирает аргументы командной строки."""
    if len(sys.argv) < 3:
        print_usage()
        sys.exit(1)

    pdf_path = sys.argv[1]
    mode = sys.argv[2]
    model_ai = sys.argv[3] if len(sys.argv) > 3 else DEFAULT_LLM_MODEL
    model_emb = sys.argv[4] if len(sys.argv) > 4 else DEFAULT_EMBEDDING_MODEL

    return pdf_path, mode, model_ai, model_emb


def run_index(pdf_path, model_emb, embeddings):
    """Режим первичного индексирования."""
    print("=" * 60)
    print(f"{datetime.now()} РЕЖИМ: Первичное индексирование PDF")
    print("=" * 60)

    chunks, metadatas = load_and_chunk_pdf(pdf_path)
    vectordb = get_or_create_vectordb(
        chunks=chunks, metadatas=metadatas,
        embeddings=embeddings, force_rebuild=True
    )

    if vectordb:
        print(f"\n{datetime.now()} Индексация завершена. Кусков в базе: {vectordb._collection.count()}")
        print("Теперь запустите: python main.py путь.pdf --search")


def run_add(pdf_path, model_emb, embeddings):
    """Режим добавления PDF в существующую базу."""
    print("=" * 60)
    print(f"{datetime.now()} РЕЖИМ: Добавление PDF в базу")
    print("=" * 60)

    vectordb = add_pdf_to_existing_db(pdf_path, embeddings)
    if vectordb:
        print(f"Готово. Всего кусков в базе: {vectordb._collection.count()}")
    else:
        print("Документ уже в базе или произошла ошибка.")


def run_search(pdf_path, model_ai, model_emb, embeddings):
    """Режим поиска и извлечения атрибутов."""
    print("=" * 60)
    print(f"{datetime.now()} РЕЖИМ: Поиск атрибутов")
    print("=" * 60)

    vectordb = get_or_create_vectordb(embeddings=embeddings)
    if vectordb is None:
        print("База не найдена. Сначала проиндексируйте PDF: python main.py путь.pdf --index")
        sys.exit(1)

    # Проверяем наличие документа
    available_docs = list_documents_in_db(vectordb)
    if pdf_path not in available_docs:
        print(f"\n[ОШИБКА] Документ '{pdf_path}' не найден в базе.")
        print(f"Доступные: {', '.join(available_docs) if available_docs else '(нет)'}")
        sys.exit(1)

    print(f"Кусков в базе: {vectordb._collection.count()}\n")

    # Поиск по всем атрибутам
    print("Поиск релевантных фрагментов...")
    all_docs = []
    seen = set()

    for attr_name, config in ATTRIBUTE_SEARCH_CONFIGS.items():
        print(f"  [{attr_name}] -> поиск...")
        contents = combined_search(
            vectordb, config["query"], config["keywords"], pdf_path, 2
        )
        for content in contents:
            if content not in seen:
                seen.add(content)
                all_docs.append(content)

    print(f"  Найдено уникальных кусков: {len(all_docs)}")

    if not all_docs:
        print("[ОШИБКА] Ничего не найдено. Проверьте поисковые запросы в config.py.")
        sys.exit(1)

    retrieved_text = "\n\n---\n\n".join(
        [f"[КУСОК {i + 1}]\n{doc}" for i, doc in enumerate(all_docs)]
    )

    # Показываем найденные фрагменты
    print("\n" + "=" * 60)
    print(f"{datetime.now()} НАЙДЕННЫЕ ФРАГМЕНТЫ:")
    print("=" * 60)
    for i, doc in enumerate(all_docs):
        print(f"\n[Кусок {i + 1}]: {doc[:300]}...")
    print("=" * 60)

    # Извлекаем атрибуты через LLM
    result = extract_json_from_chunks(model_ai, retrieved_text, ATTRIBUTE_SEARCH_CONFIGS)

    if result:
        print("\n" + "=" * 60)
        print(f"{datetime.now()} ФИНАЛЬНЫЙ РЕЗУЛЬТАТ:")
        print("=" * 60)
        import json
        print(json.dumps(result, ensure_ascii=False, indent=2))
        print("=" * 60)
    else:
        print(f"\n{datetime.now()} [ОШИБКА] Не удалось извлечь JSON.")


def run_title_extraction(pdf_path, model_ai, model_emb):
    """Режим извлечения данных с титульных листов."""
    print("=" * 60)
    print(f"{datetime.now()} РЕЖИМ: Извлечение данных титульного листа")
    print("=" * 60)

    # 1. Читаем титульные страницы (текст или OCR)
    title_text = read_title_pages(pdf_path, max_pages=TITLE_PAGES_COUNT)

    if not title_text.strip():
        print("[ОШИБКА] Не удалось извлечь текст с титульных страниц.")
        sys.exit(1)

    # 2. Показываем извлечённый текст
    print(f"\n{'='*60}")
    print(f"{datetime.now()} ИЗВЛЕЧЁННЫЙ ТЕКСТ ТИТУЛЬНЫХ ЛИСТОВ:")
    print(f"{'='*60}")
    print(title_text[:2000])  # первые 2000 символов для обзора
    if len(title_text) > 2000:
        print(f"\n... (всего {len(title_text)} символов)")
    print(f"{'='*60}")

    # 3. Отправляем в LLM
    result = extract_title_attributes_with_llm(model_ai, title_text, TITLE_ATTRIBUTES)

    if result:
        print("\n" + "=" * 60)
        print(f"{datetime.now()} АТРИБУТЫ ТИТУЛЬНОГО ЛИСТА:")
        print("=" * 60)
        import json
        print(json.dumps(result, ensure_ascii=False, indent=2))
        print("=" * 60)
    else:
        print(f"\n{datetime.now()} [ОШИБКА] Не удалось извлечь JSON для титульного листа.")


def main():
    pdf_path, mode, model_ai, model_emb = parse_args()

    print(f"Модель LLM: {model_ai}")
    print(f"Модель эмбеддингов: {model_emb}")
    print(f"{datetime.now()} Инициализация модели эмбеддингов ({model_emb})...")
    embeddings = OllamaEmbeddings(model=model_emb)
    print("Готово.\n")

    if mode == "--index":
        run_index(pdf_path, model_emb, embeddings)
    elif mode == "--add":
        run_add(pdf_path, model_emb, embeddings)
    elif mode == "--search":
        run_search(pdf_path, model_ai, model_emb, embeddings)
    elif mode == "--title":                         
        run_title_extraction(pdf_path, model_ai, model_emb)
    else:
        print(f"Неизвестный режим: {mode}")
        print_usage()
        sys.exit(1)


if __name__ == "__main__":
    main()


# ============================================================
# ГЛАВНЫЙ БЛОК
# ============================================================
# if __name__ == "__main__":
#     # Проверка аргументов командной строки
#     if len(sys.argv) < 4:
#         print("Использование:")
#         print("  Первый запуск с PDF:     python extract_attributes_rag.py модель_ИИ модель_эмбеддингов путь_к_диплому.pdf --index")
#         print("  Добавить PDF в базу:     python extract_attributes_rag.py модель_ИИ модель_эмбендинга путь_к_диплому.pdf --add")
#         print("  Только поиск по базе:    python extract_attributes_rag.py модель_ИИ модель_эмбендинга путь_к_диплому.pdf --search")
#         sys.exit(1)

#     model_ai = sys.argv[1]
#     model_embeddings = sys.argv[2]
#     pdf_path = sys.argv[3]
#     mode = sys.argv[4]

#     if not os.path.exists(pdf_path) and mode != "--search":
#         print(f"Файл '{pdf_path}' не найден.")
#         sys.exit(1)

#     # Инициализируем модель эмбеддингов ОДИН раз
#     print(f"{datetime.now()} Инициализация модели эмбеддингов ({model_embeddings})...")
#     embeddings = OllamaEmbeddings(model=model_embeddings)
#     print("Готово.\n")

#     # === РЕЖИМ 1: ПЕРВОЕ ИНДЕКСИРОВАНИЕ (создать базу с нуля) ===
#     if mode == "--index":
#         print("=" * 60)
#         print(f"{datetime.now()} РЕЖИМ: Первичное индексирование PDF")
#         print("=" * 60)
#         chunks, metadatas = load_and_chunk_pdf(pdf_path)
#         vectordb = get_or_create_vectordb(
#             chunks=chunks,
#             metadatas=metadatas,
#             embeddings=embeddings,
#             force_rebuild=True
#         )
#         if (vectordb is not None):
#             print(f"\n{datetime.now()} Индексация завершена. База сохранена в '{CHROMA_DIR}'.")
#             print(f"Всего кусков в базе: {vectordb._collection.count()}")
#             print("\nТеперь запустите скрипт в режиме --search для извлечения атрибутов.")
#         sys.exit(0)

#     # === РЕЖИМ 2: ДОБАВЛЕНИЕ PDF В СУЩЕСТВУЮЩУЮ БАЗУ ===
#     elif mode == "--add":
#         print("=" * 60)
#         print(f"{datetime.now()} РЕЖИМ: Добавление PDF в существующую базу")
#         print("=" * 60)
#         if not os.path.exists(CHROMA_DIR) or not os.listdir(CHROMA_DIR):
#             print(f"{datetime.now()} База не найдена. Сначала создайте её с флагом --index.")
#             sys.exit(1)
#         vectordb = add_pdf_to_existing_db(pdf_path, embeddings)
#         if (vectordb is not None):
#             print(f"\n{datetime.now()} Готово. Всего кусков в базе: {vectordb._collection.count()}")
#             print("Теперь запустите скрипт в режиме --search для извлечения атрибутов.")
#         else:
#             print(f"\n{datetime.now()} Данный документ уже в базе")
#             print("Вы можете запустить скрипт в режиме --search для извлечения атрибутов.")
#         sys.exit(0)

#     # === РЕЖИМ 3: ПОИСК И ИЗВЛЕЧЕНИЕ АТРИБУТОВ (основной режим) ===
#     elif mode == "--search":
#         print("=" * 60)
#         print(f"{datetime.now()} РЕЖИМ: Поиск атрибутов по существующей базе")
#         print("=" * 60)

#         # Загружаем существующую базу
#         vectordb = get_or_create_vectordb(embeddings=embeddings)
#         if vectordb is None:
#             print(f"{datetime.now()} База не найдена. Сначала проиндексируйте PDF с флагом --index.")
#             sys.exit(1)

#         # Проверяем, есть ли такой документ в базе
#         available_docs = list_documents_in_db(vectordb)
#         if pdf_path not in available_docs:
#             print(f"\n{datetime.now()} [ОШИБКА] Документ '{pdf_path}' не найден в базе.")
#             print(f"Доступные документы: {', '.join(available_docs) if available_docs else '(нет)'}")
#             print("Используйте 'ALL' для поиска по всем документам.")
#             sys.exit(1)
        
#         print(f"{datetime.now()} База загружена. Кусков в базе: {vectordb._collection.count()}\n")

#         filter_dict = {"full_path": filter}
#         all_chunks = vectordb.get()['documents']
            
#         # Словарь поисковых запросов для каждого атрибута.
#         # !!! НАСТРОЙТЕ ПОД СВОИ ДОКУМЕНТЫ !!!
#         # Добавляйте слова, которые ВСТРЕЧАЮТСЯ РЯДОМ с нужным атрибутом.
#         attribute_search_configs = {
#             "среда разработки": {
#                 "query": "среда разработки программный продукт разработан",
#                 "keywords": ["среда", "разработан", "программный", "платформа", "серверная", "клиентская"]
#             },
#             "язык программирования": {
#                 "query": "язык программирования на языке языка",
#                 "keywords": ["разработан", "написан", "код", "языка", "язык"]
#             }
#             # "blueprint_count": (
#             #     "количество чертежей листов чертёж графическая часть "
#             #     "приложение спецификация лист"
#             # ),
#             # "group_name": (
#             #     "группа студент учебная группа шифр группы "
#             #     "обучающийся выполнил студент группы"
#             # )
#         }

#         # === МУЛЬТИ-ЗАПРОСНЫЙ ПОИСК ===
#         print(f"{datetime.now()} Поиск релевантных фрагментов по каждому атрибуту...")
#         all_retrieved_docs = []
#         seen_contents = set()
        
#         added = 0
#         for attr_name, config in attribute_search_configs.items():
#             print(f"  [{attr_name}] -> гибридный поиск...")
#             contents = combined_search(vectordb, config["query"], config["keywords"], pdf_path, 2)
#             for content in contents:
#                 if content not in seen_contents:
#                     all_retrieved_docs.append(content)
#                     seen_contents.add(content)
#                     added += 1
#         print(f"    Найдено и добавлено уникальных кусков: {added}")

#         if not all_retrieved_docs:
#             print(f"\n{datetime.now()} [ОШИБКА] Ничего не найдено. Проверьте поисковые запросы.")
#             sys.exit(1)

#         # Объединяем найденные куски
#         retrieved_text = "\n\n---\n\n".join(
#             [f"[КУСОК {i+1}]\n{d}" 
#              for i, d in enumerate(all_retrieved_docs)]
#         )

#         print(f"\n{datetime.now()} Всего уникальных кусков: {len(all_retrieved_docs)}")
#         print(f"Суммарный объём текста для LLM: {len(retrieved_text)} символов")

#         # === ВЫВОД НАЙДЕННЫХ КУСКОВ (для отладки) ===
#         print("\n" + "=" * 60)
#         print(f"{datetime.now()} НАЙДЕННЫЕ ФРАГМЕНТЫ (первые 300 символов каждого):")
#         print("=" * 60)
#         for i, doc in enumerate(all_retrieved_docs):
#             preview = doc #[:300].replace('\n', ' ')
#             print(f"\n[Кусок {i+1}]: {preview}...")
#         print("\n" + "=" * 60)

#         # === ИЗВЛЕЧЕНИЕ АТРИБУТОВ ЧЕРЕЗ LLM ===
#         result = extract_json_from_chunks(model_ai, retrieved_text, attribute_search_configs)

#         # === ВАЛИДАЦИЯ И ФИНАЛЬНЫЙ ВЫВОД ===
#         if result:
#             print("\n" + "=" * 60)
#             print(f"{datetime.now()} ФИНАЛЬНЫЙ РЕЗУЛЬТАТ:")
#             print("=" * 60)
#             print(json.dumps(result, ensure_ascii=False, indent=2))
#             print("=" * 60)

#             # Дополнительная проверка: если все значения null
#             all_null = all(v is None for v in result.values())
#             if all_null:
#                 print("\n[ВНИМАНИЕ] Все атрибуты = null.")
#                 print("Возможные причины:")
#                 print("  1. Поисковые запросы не нашли нужные фрагменты.")
#                 print("  2. В найденных фрагментах действительно нет этих данных.")
#                 print("  3. LLM не смогла распознать значения (проверьте сырой ответ выше).")
#                 print("\nРекомендация: скопируйте найденные фрагменты (выше) и")
#                 print("посмотрите, есть ли в них нужные атрибуты. Если есть —")
#                 print("попробуйте заменить модель на 'mistral'.")
#         else:
#             print(f"\n{datetime.now()} [ОШИБКА] Не удалось извлечь JSON.")
#             print("Проверьте сырой ответ модели (выше).")
#             print("Возможно, нужно заменить модель на 'mistral' или 'qwen2.5'.")

#     else:
#         print(f"Неизвестный режим: {mode}")
#         print("Используйте --index, --add или --search")
#         sys.exit(1)