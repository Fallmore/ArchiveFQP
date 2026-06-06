"""
Стратегии поиска: гибридный (семантический + ключевые слова) и BM25.
"""

import re
from rank_bm25 import BM25Okapi
from langchain_chroma import Chroma

from config import HYBRID_K_SEMANTIC, HYBRID_K_FINAL, BM25_K
from vector_store import get_chunks_for_document

def hybrid_search(vectordb: Chroma,
    query: str,
    keywords: list[str],
    doc_filter: str,
    k_semantic: int = HYBRID_K_SEMANTIC,
    k_final: int = HYBRID_K_FINAL
) -> list:
    """
    Гибридный поиск: семантический + фильтрация по ключевым словам.
    
    Args:
        query: семантический запрос.
        keywords — список фраз, которые ДОЛЖНЫ быть в куске (хотя бы одна).
        doc_filter: полный путь к документу для фильтрации (или None)..
        k_semantic: количество документов, возвращаемых в семантическом поиске.
        k_final: количество документов, возвращаемых гибридным (данным) поиском
    .
    
    """
    # Шаг 1: семантический поиск (берём с запасом)
    filter_dict = {"full_path": doc_filter}
    semantic_docs = vectordb.similarity_search_with_relevance_scores(query, k=k_semantic, filter=filter_dict)
    
    # Шаг 2: фильтруем по ключевым словам
    keyword_docs = []
    other_docs = []
    
    for doc, score in semantic_docs:
        content_lower = doc.page_content.lower()
        # Проверяем, есть ли в куске хотя бы одно ключевое слово
        if any(kw.lower() in content_lower for kw in keywords):
            keyword_docs.append((doc, score, True))  # True = прошёл фильтр
        else:
            other_docs.append((doc, score, False))
    
    # Шаг 3: приоритет — отфильтрованные, потом остальные
    result_docs = [] 
    
    # Сначала добавляем те, что прошли фильтр (не больше k_final)
    for doc, score, _ in keyword_docs[:k_final]:
        result_docs.append(doc)
    
    # Если не хватило — добираем из остальных
    remaining = k_final - len(result_docs)
    if remaining > 0:
        for doc, score, _ in other_docs[:remaining]:
            result_docs.append(doc)
    
    return result_docs

def bm25_search(chunks: list[str], query: str, k: int = BM25_K) -> list[str]:
    """
    Полнотекстовый поиск BM25.
    """
    # Токенизация: разбиваем на слова, убираем знаки препинания
    def tokenize(text):
        return re.findall(r'\w+', text.lower())
    
    tokenized_chunks = [tokenize(chunk) for chunk in chunks]
    tokenized_query = tokenize(query)
    
    bm25 = BM25Okapi(tokenized_chunks)
    scores = bm25.get_scores(tokenized_query)
    
    # Индексы лучших кусков
    top_indices = sorted(range(len(scores)), key=lambda i: scores[i], reverse=True)[:k]
    return [chunks[i] for i in top_indices if scores[i] > 0]

def combined_search(vectordb: Chroma,
    query: str,
    keywords: list[str],
    doc_filter: str,
    k_final: int = HYBRID_K_FINAL
) -> tuple[str, ...]:

    """
    Объединяет гибридный (Chroma + keywords) и буквальный (BM25) поиск.
    """
    results: dict[str, int] = {}
    
    # 1. Семантический + ключевые слова (из hybrid_search)
    semantic_docs = hybrid_search(vectordb, query, keywords, doc_filter, k_semantic=10, k_final=5)
    for doc in semantic_docs:
        results[doc.page_content] = results.get(doc.page_content, 0) + 2  # вес 2
    
    doc_chunks, _ = get_chunks_for_document(vectordb, doc_filter)
    if doc_chunks:
        # 2. BM25 — буквальный поиск по точным словам
        bm25_docs = bm25_search(doc_chunks, query, k=5)
        for doc_text in bm25_docs:
            results[doc_text] = results.get(doc_text, 0) + 3  # вес 3 — выше приоритет
    else:
        print(f"   [BM25] Нет чанков для поиска (документ не найден в базе)")
    # Сортируем по суммарному весу
    sorted_docs = sorted(results.items(), key=lambda x: x[1], reverse=True)
    
    # Возвращаем тексты лучших кусков
    return tuple(text for text, _ in sorted_docs[:k_final])
