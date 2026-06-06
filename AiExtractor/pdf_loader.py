"""
Загрузка PDF-файлов и разбиение на чанки с метаданными.
"""

import os
from pypdf import PdfReader
from langchain_text_splitters import RecursiveCharacterTextSplitter

from config import CHUNK_SIZE, CHUNK_OVERLAP

def load_and_chunk_pdf(pdf_path: str, short_path: str | None = None):
    """
    Читает PDF, разбивает на осмысленные куски и возвращает список частей.
    
     Args:
        pdf_path: полный путь к PDF-файлу.
        short_path: короткий идентификатор для метаданных (если None — берётся из pdf_path).

    Returns:
        tuple: список чанков, список метаданных
    """
    print(f"1. Загружаю PDF: {pdf_path}")
    reader = PdfReader(pdf_path)
    full_text = ""
    for i, page in enumerate(reader.pages):
        page_text = page.extract_text()
        if page_text:
            full_text += page_text + "\n"
    print(f"   Всего извлечено символов: {len(full_text)}")

    # Разбиваем на куски с перекрытием.
    # RecursiveCharacterTextSplitter пытается резать по абзацам и предложениям,
    # чтобы не разрывать мысли посередине.
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=CHUNK_SIZE,
        chunk_overlap=CHUNK_OVERLAP,
        separators=["\n\n", "\n", ". ", " ", ""]
    )
    chunks = splitter.split_text(full_text)
    
    filename = os.path.splitext(os.path.basename(pdf_path))[0]
    metadatas = [{"source": filename, "full_path": short_path if short_path is not None else pdf_path} for _ in chunks]
    
    print(f"2. Разбито на {len(chunks)} кусков (размер ~{CHUNK_SIZE} символов, перекрытие {CHUNK_OVERLAP}).")
    return chunks, metadatas