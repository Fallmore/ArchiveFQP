"""
Асинхронный клиент для Ollama API с поддержкой отмены.
"""

import asyncio
import json
import httpx
from typing import Optional

from config import (
    OLLAMA_HOST,
    OLLAMA_TIMEOUT
)


async def ollama_generate_async(
    model: str,
    system: str,
    prompt: str,
    temperature: float = 0,
    num_predict: int = 512,
    cancellation_event: Optional[asyncio.Event] = None
) -> str:
    """
    Асинхронная генерация через Ollama API с возможностью отмены.

    Args:
        model: имя модели.
        system: системный промпт.
        prompt: пользовательский запрос.
        temperature: температура генерации.
        num_predict: макс. число токенов.
        cancellation_event: если установлен, проверяется между токенами.

    Returns:
        str: полный ответ модели.
    """
    payload = {
        "model": model,
        "system": system,
        "prompt": prompt,
        "stream": False,  # отключаем стриминг — получаем ответ целиком
        "options": {
            "temperature": temperature,
            "num_predict": num_predict
        }
    }

    async with httpx.AsyncClient(timeout=OLLAMA_TIMEOUT) as client:
        # Если есть событие отмены — проверяем его
        if cancellation_event:
            if cancellation_event.is_set():
                raise asyncio.CancelledError("Отменено до запроса")

        response = await client.post(
            f"{OLLAMA_HOST}/api/generate",
            json=payload
        )

        if response.status_code != 200:
            raise ConnectionError(f"Ollama error: {response.status_code} - {response.text}")

        data = response.json()
        return data.get("response", "")


async def ollama_chat_async(
    model: str,
    messages: list[dict],
    temperature: float = 0,
    num_predict: int = 512,
    cancellation_event: Optional[asyncio.Event] = None
) -> str:
    """
    Асинхронный чат через Ollama API (альтернатива ollama.chat()).

    Args:
        model: имя модели.
        messages: список сообщений [{"role": "system/user/assistant", "content": "..."}].
        temperature: температура.
        num_predict: макс. токенов.
        cancellation_event: событие отмены.

    Returns:
        str: ответ модели.
    """
    payload = {
        "model": model,
        "messages": messages,
        "stream": False,
        "options": {
            "temperature": temperature,
            "num_predict": num_predict
        }
    }

    async with httpx.AsyncClient(timeout=OLLAMA_TIMEOUT) as client:
        if cancellation_event and cancellation_event.is_set():
            raise asyncio.CancelledError("Отменено до запроса")

        response = await client.post(
            f"{OLLAMA_HOST}/api/chat",
            json=payload
        )

        if response.status_code != 200:
            raise ConnectionError(f"Ollama error: {response.status_code} - {response.text}")

        data = response.json()
        return data.get("message", {}).get("content", "")
    

# async def stop_ollama_generation():
#     """
#     Отправляет Ollama команду остановить текущую генерацию.
#     У Ollama нет отдельного эндпоинта на отмену конкретного запроса,
#     но POST /api/generate с action=stop завершает все активные генерации.
#     """
#     try:
#         async with httpx.AsyncClient() as client:
#             # Пробуем основной способ — остановка конкретного запроса
#             # (работает если Ollama поддерживает stop по id)
#             response = await client.post(
#                 f"{OLLAMA_API}/api/stop",
#                 timeout=3.0
#             )
#             print(f"  [Ollama stop] Ответ: {response.status_code} - {response.text[:100]}")
#     except Exception as e:
#         print(f"  [Ollama stop] Предупреждение: {e}")