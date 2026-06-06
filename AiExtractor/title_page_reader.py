"""
Чтение титульных листов (первых 5 страниц) PDF.
Поддерживает и текст, и сканы/фото (через OCR).
"""

from datetime import datetime
import os
import tempfile
from typing import Optional
from pypdf import PdfReader
from pdf2image import convert_from_path
import pytesseract
from PIL import Image, ImageFilter, ImageEnhance, ImageOps
import cv2
import numpy as np

from config import (
    TESSERACT_LANG,
    TESSERACT_CONFIG,
    MIN_TEXT_CHARS_PER_PAGE,
    TITLE_PAGES_COUNT
)
from llm_extractor import _parse_json
from ollama_async import ollama_chat_async

def _extract_text_from_page(page) -> str:
    """Извлекает текст из одной страницы PDF (через pypdf)."""
    text = page.extract_text()
    return text.strip() if text else ""


def _is_text_page(text: str) -> bool:
    """
    Определяет, содержит ли страница осмысленный текст.
    Если символов меньше порога — вероятно, это скан/фото без текстового слоя.
    """
    # Убираем пробелы и переносы строк для подсчёта "полезных" символов
    clean = text.replace(" ", "").replace("\n", "").replace("\r", "")
    return len(clean) >= MIN_TEXT_CHARS_PER_PAGE


def preprocess_for_ocr(
    image: Image.Image,
    save_debug: bool = False,
    debug_dir: str = "./debug_ocr"
) -> Image.Image:
    """
    Предобработка изображения для улучшения распознавания OCR.
    При save_debug=True сохраняет все промежуточные этапы в отдельные файлы.

    Args:
        image: исходное изображение (PIL Image).
        save_debug: сохранять ли промежуточные этапы обработки.
        debug_dir: папка для отладочных изображений.

    Returns:
        PIL Image: обработанное чёрно-белое изображение.
    """
    import time

    # Создаём папку для отладки, если нужно
    if save_debug:
        os.makedirs(debug_dir, exist_ok=True)
        timestamp = int(time.time() * 1000)  # уникальный идентификатор
        print(f"\n    [DEBUG] Сохраняю этапы обработки в '{debug_dir}/' (префикс: {timestamp})")

        # Сохраняем оригинал
        original_path = os.path.join(debug_dir, f"{timestamp}_01_original.png")
        image.save(original_path)
        print(f"    [DEBUG]   Сохранён оригинал: {original_path}")

    # PIL -> numpy
    img = np.array(image)

    # === 1. Оттенки серого ===
    if len(img.shape) == 3:
        gray = cv2.cvtColor(img, cv2.COLOR_RGB2GRAY)
    else:
        gray = img.copy()

    if save_debug:
        gray_path = os.path.join(debug_dir, f"{timestamp}_02_grayscale.png")
        Image.fromarray(gray).save(gray_path)
        print(f"    [DEBUG]   Сохранён серый: {gray_path}")
        
    # # === 2. CLAHE ===
    # clahe = cv2.createCLAHE(clipLimit=2.0, tileGridSize=(8, 8))
    # enhanced = clahe.apply(gray)

    # if save_debug:
    #     clahe_path = os.path.join(debug_dir, f"{timestamp}_03_clahe.png")
    #     Image.fromarray(enhanced).save(clahe_path)
    #     print(f"    [DEBUG]   Сохранён CLAHE: {clahe_path}")

    # === 3. Бинаризация ===
    _, otsu = cv2.threshold(gray, 0, 255, cv2.THRESH_BINARY + cv2.THRESH_OTSU)
    adaptive = cv2.adaptiveThreshold(
        gray, 255,
        cv2.ADAPTIVE_THRESH_GAUSSIAN_C,
        cv2.THRESH_BINARY,
        blockSize=51,
        C=10
    )

    if save_debug:
        otsu_path = os.path.join(debug_dir, f"{timestamp}_04a_otsu.png")
        adaptive_path = os.path.join(debug_dir, f"{timestamp}_04b_adaptive.png")
        Image.fromarray(otsu).save(otsu_path)
        Image.fromarray(adaptive).save(adaptive_path)
        print(f"    [DEBUG]   Сохранён Оцу: {otsu_path}")
        print(f"    [DEBUG]   Сохранён адаптивный: {adaptive_path}")

    # === 4. Выбор лучшего метода ===
    def black_pixel_ratio(bin_img):
        return np.sum(bin_img == 0) / bin_img.size

    otsu_ratio = black_pixel_ratio(otsu)
    adaptive_ratio = black_pixel_ratio(adaptive)

    if 0.03 < adaptive_ratio < 0.20:
        binary = adaptive
        method_used = "адаптивный порог"
    elif 0.03 < otsu_ratio < 0.20:
        binary = otsu
        method_used = "метод Оцу"
    elif adaptive_ratio < otsu_ratio:
        binary = adaptive
        method_used = "адаптивный порог (запасной)"
    else:
        binary = otsu
        method_used = "метод Оцу (запасной)"

    print(f"    [OCR prep] Чёрных пикселей: Оцу={otsu_ratio:.1%}, Адапт.={adaptive_ratio:.1%} → выбран {method_used}")

    if save_debug:
        chosen_path = os.path.join(debug_dir, f"{timestamp}_05_chosen_{method_used.replace(' ', '_')}.png")
        Image.fromarray(binary).save(chosen_path)
        print(f"    [DEBUG]   Выбранный метод: {chosen_path}")

    # === 5. Медианный фильтр ===
    denoised = cv2.medianBlur(binary, 5)

    if save_debug:
        denoised_path = os.path.join(debug_dir, f"{timestamp}_06_denoised.png")
        Image.fromarray(denoised).save(denoised_path)
        print(f"    [DEBUG]   Сохранён после шумоподавления: {denoised_path}")

    # === 6. Морфология ===
    # Этап 6a: Горизонтальное закрытие (спасает "е", "ё", "а", "б", "в", "ы")
    kernel_h = cv2.getStructuringElement(cv2.MORPH_RECT, (5, 1))
    closed_h = cv2.morphologyEx(denoised, cv2.MORPH_CLOSE, kernel_h)

    # Этап 6b: Лёгкое вертикальное закрытие (спасает "н", "и", "п" от разрывов)
    kernel_v = cv2.getStructuringElement(cv2.MORPH_RECT, (1, 3))
    morphed = cv2.morphologyEx(closed_h, cv2.MORPH_CLOSE, kernel_v)

    if save_debug:
        morphed_path = os.path.join(debug_dir, f"{timestamp}_07_morphology.png")
        Image.fromarray(morphed).save(morphed_path)
        print(f"    [DEBUG]   Сохранён после морфологии: {morphed_path}")

    # === 7. Инверсия ===
    if morphed.mean() < 128:
        morphed = cv2.bitwise_not(morphed)
        if save_debug:
            inverted_path = os.path.join(debug_dir, f"{timestamp}_08_inverted.png")
            Image.fromarray(morphed).save(inverted_path)
            print(f"    [DEBUG]   Сохранён после инверсии: {inverted_path}")

    # === 8. Резкость ===
    pil_img = Image.fromarray(morphed)
    enhancer = ImageEnhance.Sharpness(pil_img)
    pil_img = enhancer.enhance(1.5)

    if save_debug:
        final_path = os.path.join(debug_dir, f"{timestamp}_09_final.png")
        pil_img.save(final_path)
        print(f"    [DEBUG]   Финальный результат: {final_path}")
        print(f"    [DEBUG] Все этапы сохранены. Откройте папку '{debug_dir}' и сравните.\n")

    return pil_img


def _ocr_page(image: Image.Image) -> str:
    """Распознаёт текст с изображения страницы через Tesseract OCR."""
    processed = preprocess_for_ocr(image)

    # Распознавание
    text = pytesseract.image_to_string(
        image,
        lang=TESSERACT_LANG,
        config=TESSERACT_CONFIG
    )
    return text.strip()





def read_title_pages(pdf_path: str, max_pages: int = TITLE_PAGES_COUNT) -> str:
    """
    Читает первые max_pages страниц PDF.
    Для каждой страницы:
    - Пытается извлечь текст через pypdf.
    - Если текст пустой или мусорный (< MIN_TEXT_CHARS_PER_PAGE) —
      рендерит страницу как изображение и применяет OCR.

    Args:
        pdf_path: путь к PDF-файлу.
        max_pages: сколько первых страниц обработать.

    Returns:
        str: объединённый текст всех обработанных страниц.
    """
    print(f"\n{'='*60}")
    print(f"Чтение титульных листов (первые {max_pages} стр.): {pdf_path}")
    print(f"{'='*60}")

    all_texts = []

    # === ЭТАП 1: пробуем текстовое извлечение ===
    reader = PdfReader(pdf_path)
    total_pages = len(reader.pages)
    pages_to_check = min(max_pages, total_pages)

    # Список страниц, которые нужно отправить на OCR
    pages_for_ocr = []

    for i in range(pages_to_check):
        page = reader.pages[i]
        text = _extract_text_from_page(page)

        if _is_text_page(text):
            print(f"  Стр. {i+1}: текст найден ({len(text)} симв.) ✓")
            if i+1 == 1:
                start = max(text.find('Институт'), 0)
                # Убираем лишние ФИО
                mid = min(text.find('ученая степень'), text.find('ученое звание'), text.find(', Фамилия'), text.find('Консультант'))
                if mid == -1: mid = len(text)-1 
                end = text.rfind('Астрахань')
                text = text[start:mid]
                if end != -1 and mid != len(text)-1: text += text[end:]
            all_texts.append(f"\n[Страница {i+1}]\n{text}")
        else:
            print(f"  Стр. {i+1}: текста нет или мало ({len(text)} симв.) → нужен OCR")
            pages_for_ocr.append(i + 1)  # +1 для человеческой нумерации

    # === ЭТАП 2: OCR для страниц без текста ===
    if pages_for_ocr:
        print(f"\n  Запуск OCR для страниц: {pages_for_ocr}...")

        # Конвертируем ВСЕ первые max_pages в изображения (быстрее,
        # чем по одной, если страниц много под OCR)
        # first_page: номер первой страницы (1-based для pdf2image)
        images = convert_from_path(
            pdf_path,
            first_page=1,
            last_page=pages_to_check,
            dpi=500 ,
            
        )

        for i, image in enumerate(images):
            page_num = i + 1
            if page_num in pages_for_ocr:
                ocr_text = _ocr_page(image)
                print(f"  Стр. {page_num}: OCR выполнено ({len(ocr_text)} симв.)")
                print(f"{ocr_text}")
                if page_num == 1:
                    start = max(ocr_text.find('Институт'), 0)
                    # Убираем лишние ФИО
                    mid = min(ocr_text.find('ученая степень'), ocr_text.find('ученое звание'), ocr_text.find(', Фамилия'), ocr_text.find('Консультант'))
                    if mid == -1: mid = len(ocr_text)-1 
                    end = ocr_text.rfind('Астрахань')
                    ocr_text = ocr_text[start:mid]
                    if end != -1 and mid != len(ocr_text)-1: ocr_text += ocr_text[end:]

                all_texts.append(f"\n[Страница {page_num} — OCR]\n{ocr_text}")
            # если страница уже была текстовая — вторую копию не добавляем

    result = "\n\n" + "="*40 + "\n\n".join(all_texts)
    print(f"\n  Итого извлечено: {len(result)} символов с {pages_to_check} страниц.")
    return result


def extract_title_attributes_with_llm(
    model_ai: str,
    title_text: str,
    title_attributes: dict,
) -> dict | None:
    """
    Извлекает атрибуты титульного листа через LLM.

    В отличие от основного поиска, здесь мы не используем RAG —
    просто отправляем весь текст титульных листов в модель.

    Args:
        model_ai: имя модели Ollama.
        title_text: текст титульных листов (из read_title_pages).
        title_attributes: конфигурация атрибутов титульника (из config.py).

    Returns:
        dict | None: распарсенный JSON с атрибутами.
    """
    from ollama import chat
    import json

    attr_names = list(title_attributes.keys())

    # === ПЕРЕЧЕНЬ АТРИБУТОВ С ПРИМЕРАМИ ===
    attr_list_lines = []
    for name, config in title_attributes.items():
        examples = config.get("examples", "")
        line = f'- "{name}"'
        if examples:
            line += f" (например, {examples})"
        attr_list_lines.append(line)
        
    # === ПОДСКАЗКИ ДЛЯ ПОИСКА ===
    hints_lines = []
    for name, config in title_attributes.items():
        hint = config.get("keywords", "")
        if hint:
            hints_lines.append(f'- "{name}": {hint}')
    
    hints_text = "\n".join(hints_lines) if hints_lines else ""

    system_msg = (
        "Ты — ассистент по извлечению данных из титульных листов дипломных работ. "
        "Верни ТОЛЬКО JSON. Если атрибут не найден — поставь 'Н/Д'."
    )

    user_msg = f"""
ИНСТРУКЦИЯ: Извлеки из текста титульных листов значения атрибутов.

АТРИБУТЫ ДЛЯ ПОИСКА:
{chr(10).join(attr_list_lines)}

ПРАВИЛА:
1. Верни JSON СТРОГО с ключами: {", ".join([f'"{n}"' for n in attr_names])}
2. Не добавляй других ключей.
3. Если не нашёл — поставь "Н/Д".
4. Не пиши ничего кроме JSON.
5. Не используй примеры из инструкции как ответ — только реальные данные из текста.
6. В руководители укажи только ФИО


Подсказки, что находится рядом со значениями:
{hints_text}

ТЕКСТ ТИТУЛЬНЫХ ЛИСТОВ:
{title_text}

ОТВЕТ (ТОЛЬКО JSON):
"""
    print(f"\n{datetime.now()} Отправляю в {model_ai}...")
    response = chat(
        model=model_ai,
        messages=[
            {'role': 'system', 'content': system_msg},
            {'role': 'user', 'content': user_msg}
        ],
        stream=False,
        options={'temperature': 0, 'num_predict': 512}
    )

    llm_answer = response['message']['content']
    print("\n----- Сырой ответ (титульник) -----")
    print(llm_answer)
    print("------------------------------------\n")

    return _parse_json(llm_answer)

    
    
async def extract_title_attributes_async(
    model_ai: str,
    title_text: str,
    title_attributes: dict,
    cancellation_event=None
) -> dict | None:
    """Асинхронное извлечение титульника с отменой."""
    attr_names = list(title_attributes.keys())

    attr_list = []
    for name, config in title_attributes.items():
        examples = config.get("examples", "")
        line = f'- "{name}"'
        if examples:
            line += f" (например, {examples})"
        attr_list.append(line)

    system_msg = (
        "Ты — ассистент по извлечению данных из титульных листов. "
        "Верни ТОЛЬКО JSON. Не найдено — ставь 'Н/Д'."
    )

    user_msg = f"""
ИНСТРУКЦИЯ: Извлеки из текста значения атрибутов.

АТРИБУТЫ:
{chr(10).join(attr_list)}

ПРАВИЛА:
1. JSON только с ключами: {", ".join(attr_names)}
2. Не найдено → "Н/Д"
3. ТОЛЬКО JSON, без лишнего текста.

ТЕКСТ ТИТУЛЬНЫХ ЛИСТОВ:
{title_text}

ОТВЕТ (ТОЛЬКО JSON):
"""

    print(f"\n{datetime.now()} Отправляю в {model_ai}...")
    answer = await ollama_chat_async(
        model=model_ai,
        messages=[
            {'role': 'system', 'content': system_msg},
            {'role': 'user', 'content': user_msg}
        ],
        temperature=0,
        num_predict=512,
        cancellation_event=cancellation_event
    )

    print("\n----- Сырой ответ (титульник) -----")
    print(answer)
    print("------------------------------------\n")

    return _parse_json(answer)
