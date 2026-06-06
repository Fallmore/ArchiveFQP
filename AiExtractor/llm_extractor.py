"""
Формирование промптов и вызов LLM через Ollama.
"""

import json
from datetime import datetime
from ollama import chat
from ollama_async import ollama_chat_async


def create_extraction_prompt(retrieved_text: str, attributes: dict) -> tuple[str, str]:
    """
    Создаёт системный и пользовательский промпты для извлечения атрибутов.

    Args:
        retrieved_text: найденный текст из PDF.
        attributes: словарь конфигураций атрибутов из config.py.

    Returns:
        tuple[str, str]: (system_message, user_message)
    """

    system_message = (
        "Ты — робот-экстрактор. Твоя единственная задача — найти в тексте "
        "конкретные значения атрибутов и вернуть JSON. "
        "Никаких «я думаю», никаких пересказов, никаких саммари. "
        "ТОЛЬКО JSON. Если не нашёл — ставь Н/Д."
    )

    attr_names = list(attributes.keys())
    
    # === ПЕРЕЧЕНЬ АТРИБУТОВ С ПРИМЕРАМИ ===
    attr_list_lines = []
    for name, config in attributes.items():
        examples = config.get("examples", "")
        if examples:
            attr_list_lines.append(f'- "{name}" (например, {examples})')
        else:
            attr_list_lines.append(f'- "{name}"')
    
    attr_list_text = "\n".join(attr_list_lines)
    
    # === ПОДСКАЗКИ ДЛЯ ПОИСКА ===
    hints_lines = []
    for name, config in attributes.items():
        hint = config.get("keywords", "")
        if hint:
            hints_lines.append(f'- "{name}": {hint}')
    
    hints_text = "\n".join(hints_lines) if hints_lines else ""

    user_message = f"""
ИНСТРУКЦИЯ: Ты должен извлечь из текста значения атрибутов и вернуть ТОЛЬКО JSON-объект.

АТРИБУТЫ ДЛЯ ПОИСКА:
{attr_list_text}

ПРАВИЛА (НАРУШАТЬ ЗАПРЕЩЕНО):
1. Верни JSON СТРОГО с ключами: {", ".join([f'"{n}"' for n in attr_names])}
2. Не создавай других ключей.
3. Если не нашёл атрибут — ставь Н/Д.
4. Не используй примеры из инструкции как ответ — только реальные данные из текста.
5. Не пиши ничего кроме JSON. Ни комментариев, ни пояснений.
6. Если у атрибута есть 2 и более ответа, впиши их все через запятую ","

ФОРМАТ ОТВЕТА (СКОПИРУЙ ТОЧНО):
{{"атрибут": "найденное или Н/Д", "атрибут": "найденное или Н/Д" ...}}

Подсказки для поиска значений:
{hints_text}

Пример 1:
Текст: '... в среде разработки WebStorm 2024.3.2 на
языке Typescript. ... был использован Node.js + TypeScript.'
Ответ: {{"среда разработки": "WebStorm 2024.3.2", "язык программирования": "Typescript,Node.js"}}
Пример 2:
Текст: '... в интегрированной среде intellij idea community edition,
... на языке Java 17, ... клиентская часть на платформе Vue языка Javascript.'
Ответ: {{"среда разработки": "IntelliJ IDEA,Vue", "язык программирования": "Java,Javascript"}}

ТЕКСТ ДЛЯ АНАЛИЗА:
{retrieved_text}

ОТВЕТ (ТОЛЬКО JSON):
"""

    return system_message, user_message


def create_extraction_prompt_dynamic(retrieved_text: str, attributes: list[dict]) -> tuple[str, str]:
    """
    Создаёт промпт на основе ДИНАМИЧЕСКОЙ конфигурации из БД.

    attributes — список словарей:
    [
        {
            "название": "среда разработки",
            "query": "среда разработки ide",
            "keywords": ["среда", "разработан", "программный", "платформа", "серверная", "клиентская"],
            "examples": ["IntelliJ IDEA", "Visual Studio"]
        },
        ...
    ]
    """
    attr_names = [a["название"] for a in attributes]

    # Перечень атрибутов с примерами
    attr_list_lines = []
    for attr in attributes:
        name = attr["название"]
        examples = attr.get("examples", [])
        if examples:
            attr_list_lines.append(f'- "{name}" (например, {", ".join(examples)})')

    # Подсказки из keywords
    hints_lines = []
    for attr in attributes:
        name = attr["название"]
        keywords = attr.get("keywords", [])
        if keywords:
            hints_lines.append(f'- "{name}": ищи рядом: {", ".join(keywords)}')

    system_message = (
        "Ты — робот-экстрактор. Твоя единственная задача — найти в тексте "
        "конкретные значения атрибутов и вернуть JSON. "
        "Никаких «я думаю», никаких пересказов, никаких саммари. "
        "ТОЛЬКО JSON. Если не нашёл — ставь 'Н/Д'."
    )

    user_message = f"""
ИНСТРУКЦИЯ: Извлеки из текста значения атрибутов и верни ТОЛЬКО JSON-объект.

АТРИБУТЫ ДЛЯ ПОИСКА:
{chr(10).join(attr_list_lines)}

ПРАВИЛА:
1. Верни JSON СТРОГО с ключами: {", ".join([f'"{n}"' for n in attr_names])}
2. Не создавай других ключей.
3. Если не нашёл атрибут — поставь "Н/Д".
4. Не используй примеры как ответ — только реальные данные из текста.
5. Не пиши ничего кроме JSON.
6. Если у атрибута есть 2 и более ответа, впиши их через запятую ",".

ФОРМАТ ОТВЕТА (СКОПИРУЙ ТОЧНО):
{{"атрибут": "найденное или Н/Д", "атрибут": "найденное или Н/Д" ...}}

Подсказки для поиска:
{chr(10).join(hints_lines) if hints_lines else "отсутствуют"}

ТЕКСТ ДЛЯ АНАЛИЗА:
{retrieved_text}

ОТВЕТ (ТОЛЬКО JSON):
"""
    return system_message, user_message



def extract_json_from_chunks(model_ai: str, retrieved_text: str, attributes: dict) -> dict | None:
    """
    Отправляет текст в LLM и получает структурированный JSON.

    Args:
        model_ai: имя модели Ollama.
        retrieved_text: текст для анализа.
        attributes: конфигурация атрибутов.

    Returns:
        dict | None: распарсенный JSON или None.
    """
    system_msg, user_msg = create_extraction_prompt(retrieved_text, attributes)

    print(f"{datetime.now()} 5. Отправляю в {model_ai}... (фрагмент размером {len(retrieved_text)} символов)")
    response = chat(
        model=model_ai,
        messages=[
            {'role': 'system', 'content': system_msg},
            {'role': 'user', 'content': user_msg}
        ],
        stream=False,
        options={
            'temperature': 0,
            'num_predict': 256
        }
    )

    llm_answer = response['message']['content']
    print("\n----- Сырой ответ модели -----")
    print(llm_answer)
    print("------------------------------\n")

    return parse_json(llm_answer)


def extract_json_from_chunks_dynamic(
    model_ai: str,
    retrieved_text: str,
    attributes: list[dict]
) -> dict | None:
    """
    Отправляет текст в LLM и получает структурированный JSON.
    attributes — динамический список из БД.
    """
    from ollama import chat

    system_msg, user_msg = create_extraction_prompt_dynamic(retrieved_text, attributes)

    print(f"{datetime.now()} 5. Отправляю в {model_ai}... (фрагмент: {len(retrieved_text)} симв.)")
    response = chat(
        model=model_ai,
        messages=[
            {'role': 'system', 'content': system_msg},
            {'role': 'user', 'content': user_msg}
        ],
        stream=False,
        options={'temperature': 0, 'num_predict': 256}
    )

    llm_answer = response['message']['content']
    print("----- Сырой ответ -----")
    print(llm_answer)
    print("------------------------")

    return _parse_json(llm_answer)



async def extract_json_from_chunks_async(
    model_ai: str,
    retrieved_text: str,
    attributes: dict,
    cancellation_event=None
) -> dict | None:
    """Асинхронная версия с отменой."""
    system_msg, user_msg = create_extraction_prompt(retrieved_text, attributes)

    print(f"{datetime.now()} 5. Отправляю в {model_ai}... (фрагмент размером {len(retrieved_text)} символов)")

    answer = await ollama_chat_async(
        model=model_ai,
        messages=[
            {'role': 'system', 'content': system_msg},
            {'role': 'user', 'content': user_msg}
        ],
        temperature=0,
        num_predict=256,
        cancellation_event=cancellation_event
    )

    print("\n----- Сырой ответ модели -----")
    print(answer)
    print("------------------------------\n")

    return _parse_json(answer)


async def extract_json_from_chunks_dynamic_async(
    model_ai: str,
    retrieved_text: str,
    attributes: list[dict],
    cancellation_event=None
) -> dict | None:
    """
    Отправляет текст в LLM и получает структурированный JSON.
    attributes — динамический список из БД.
    """

    system_msg, user_msg = create_extraction_prompt_dynamic(retrieved_text, attributes)

    print(f"{datetime.now()} 5. Отправляю в {model_ai}... (фрагмент: {len(retrieved_text)} симв.)")
    answer = await ollama_chat_async(
        model=model_ai,
        messages=[
            {'role': 'system', 'content': system_msg},
            {'role': 'user', 'content': user_msg}
        ],
        temperature=0,
        num_predict=256,
        cancellation_event=cancellation_event
    )

    print("----- Сырой ответ -----")
    print(answer)
    print("------------------------")

    return _parse_json(answer)



def parse_json(raw_answer: str) -> dict | None:
    """Извлекает JSON из ответа модели."""
    cleaned = raw_answer.strip()
    start = cleaned.find('{')
    end = cleaned.rfind('}')
    if start == -1 or end == -1:
        print("В ответе нет JSON-объекта.")
        return None
    
    try:
        return json.loads(cleaned[start:end+1])
    except json.JSONDecodeError as e:
        print(f"Ошибка JSON: {e}")
        print(f"Пытался распарсить: {cleaned[start:end+1]}")
        return None


def _parse_json(raw: str) -> dict | None:
    cleaned = raw.strip()
    start = cleaned.find('{')
    end = cleaned.rfind('}')
    if start == -1 or end == -1:
        return None
    try:
        return json.loads(cleaned[start:end + 1])
    except json.JSONDecodeError:
        return None