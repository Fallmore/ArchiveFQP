"""
Работа с векторной базой данных Chroma: создание, загрузка, добавление документов.
"""

import os
import shutil
from langchain_chroma import Chroma

from config import CHROMA_DIR
from pdf_loader import load_and_chunk_pdf

def get_or_create_vectordb(chunks: list[str] | None = None,
    metadatas: list[dict] | None = None,
    embeddings=None,
    force_rebuild: bool = False
) -> Chroma | None: 
    """
    Загружает существующую базу Chroma или создаёт новую.
    
    Args:
        chunks: список текстов чанков (для новой базы).
        metadatas: список метаданных (для новой базы).
        embeddings: модель эмбеддингов.
        force_rebuild: удалить старую базу и создать заново.

    Returns:
        Chroma: Объект базы данных или None, если нет данных.
    """
    if force_rebuild and chunks is not None:
        print("   Принудительная перестройка базы...")
        if os.path.exists(CHROMA_DIR):
            print(f"   Удаляю старую базу: '{CHROMA_DIR}'...")
            shutil.rmtree(CHROMA_DIR)
            print("   Старая база удалена.")
            
        vectordb = Chroma.from_texts(
            texts=chunks,
            embedding=embeddings,
            metadatas=metadatas,
            persist_directory=CHROMA_DIR
        )
        return vectordb

    # Пробуем загрузить существующую
    if os.path.exists(CHROMA_DIR) and os.listdir(CHROMA_DIR):
        print(f"   Загружаю существующую базу из '{CHROMA_DIR}'...")
        vectordb = Chroma(
            persist_directory=CHROMA_DIR,
            embedding_function=embeddings
        )
        # Если нужно добавить ещё документов — эта база уже готова к поиску
        return vectordb
    else:
        if (chunks is not None):
            print(f"   База не найдена. Создаю новую...")
            vectordb = Chroma.from_texts(
                texts=chunks,
                embedding=embeddings,
                metadatas=metadatas,
                persist_directory=CHROMA_DIR
            )
            return vectordb
        else:
            return None

def list_documents_in_db(vectordb: Chroma) -> list[str]:
    """
    Возвращает список уникальных путей (full_path) документов в базе.
    """
    # Получаем все метаданные из коллекции
    collection_data = vectordb.get()
    metadatas = collection_data.get('metadatas', [])
    
    sources = set()
    for meta in metadatas:
        if meta and 'full_path' in meta:
            sources.add(meta['full_path'])
    
    return sorted(list(sources))

def add_pdf_to_existing_db(pdf_path: str, embeddings, short_path: str | None = None) -> Chroma | None:
    """
    Добавляет новый PDF в существующую базу (без дубликатов).
    
    Returns:
        Chroma: базу, если документ добавлен; None, если уже существует.
    """
    
    vectordb = Chroma(
        persist_directory=CHROMA_DIR,
        embedding_function=embeddings
    )
    
    available_docs = list_documents_in_db(vectordb)
    if pdf_path in available_docs or short_path in available_docs:
        return None
            
    print(f"   Добавляю в базу: {pdf_path}")
    chunks, metadatas = load_and_chunk_pdf(pdf_path, short_path)

    
    # Генерируем ID для новых кусков (чтобы не пересекались)
    existing_count = vectordb._collection.count()
    ids = [str(i + existing_count) for i in range(len(chunks))]
    vectordb.add_texts(chunks, metadatas, ids)
    print(f"   Добавлено. Всего кусков в базе: {vectordb._collection.count()}")
    return vectordb

def get_chunks_for_document(vectordb: Chroma, doc_filter: str | None = None) -> tuple[list[str], list[dict]]:
    """
    Возвращает все чанки из базы. Если doc_filter указан — только для этого документа.

    Args:
        vectordb: объект Chroma.
        doc_filter: полный путь к документу для фильтрации (или None).

    Returns:
        tuple: (тексты чанков, метаданные)
    """
    # Получаем все данные из коллекции Chroma
    collection_data = vectordb.get()
    
    all_texts = collection_data.get('documents', [])
    all_metadatas = collection_data.get('metadatas', [])
    
    if doc_filter is None:
        return all_texts, all_metadatas
    
    # Фильтруем по имени документа
    filtered_texts = []
    filtered_metadatas = []
    
    for text, meta in zip(all_texts, all_metadatas):
        if meta and meta.get('full_path') == doc_filter:
            filtered_texts.append(text)
            filtered_metadatas.append(meta)
    
    print(f"   [Фильтр] Всего чанков в базе: {len(all_texts)}")
    print(f"   [Фильтр] Чанков для '{doc_filter}': {len(filtered_texts)}")
    
    return filtered_texts, filtered_metadatas
