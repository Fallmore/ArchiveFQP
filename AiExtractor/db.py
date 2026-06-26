"""
Работа с PostgreSQL: загрузка конфигураций атрибутов и сохранение результатов.
"""

import json
import os
from datetime import datetime
from typing import Optional

import asyncpg
import psycopg2
from psycopg2.extras import RealDictCursor

from config import (
    DSN
)


# ============================================================
# Асинхронное подключение (для FastAPI)
# ============================================================
async def get_async_connection():
    """Создаёт асинхронное подключение к PostgreSQL."""
    return await asyncpg.connect(DSN)


# ============================================================
# Синхронное подключение (для фоновых задач в executor)
# ============================================================
def get_sync_connection():
    """Создаёт синхронное подключение к PostgreSQL."""
    return psycopg2.connect(DSN)


# ============================================================
# Загрузка конфигурации атрибутов по id_работы
# ============================================================

async def get_pending_attributes_for_work(work_id: int) -> list[dict]:
    """
    Находит ВСЕ записи в данные_по_атриб, помеченные как 'Ожидание поиска...',
    для указанной работы. Возвращает полную информацию:
    id_данных, id_работы, id_атрибута, название атрибута, настройки (JSON),
    и текущее значение данные.
    
    Логика запроса:
    1. Из данные_по_атриб берём записи с id_работы = work_id
       и данные = 'Ожидание поиска...'
    2. Через id_структуры → атрибут_учреждения → id_атрибута → атрибут
    3. Получаем название атрибута и его настройки (поисковый запрос, ключевые слова)
    """
    conn = await get_async_connection()
    try:
        rows = await conn.fetch("""
            SELECT 
                д.id_данных,
                д.id_работы,
                д.данные AS текущее_значение,
                а.id_атрибута,
                а.название,
                а.настройки
            FROM данные_по_атриб д
            JOIN атрибут_учреждения ау ON д.id_структуры = ау.id_структуры
            JOIN атрибут а ON ау.id_атрибута = а.id_атрибута
            WHERE д.id_работы = $1
              AND д.данные = 'Ожидание поиска...'
            ORDER BY а.id_атрибута
        """, work_id)

        result = []
        for row in rows:
            settings = json.loads(row["настройки"]) if row["настройки"] else {}

            result.append({
                "id_данных": row["id_данных"],
                "id_работы": row["id_работы"],
                "id_атрибута": row["id_атрибута"],
                "название": row["название"],
                "текущее_значение": row["текущее_значение"],
                # Поля из настроек JSON
                "query": settings.get("query", ""),
                "keywords": settings.get("keywords", []),
                "examples": settings.get("examples", ""),
            })

        return result
    finally:
        await conn.close()


# ============================================================
# Сохранение найденных данных
# ============================================================

async def save_attribute_data(data_id: int, value: str):
    """
    Обновляет запись в данные_по_атриб найденным значением.
    """
    conn = await get_async_connection()
    try:
        await conn.execute("""
            UPDATE данные_по_атриб
            SET данные = $1
            WHERE id_данных = $2
        """, value, data_id)
    finally:
        await conn.close()


async def save_multiple_attributes(updates: list[dict]):
    """
    Массовое обновление записей.
    updates: список [{"id_данных": 1, "данные": "значение"}, ...]
    """
    conn = await get_async_connection()
    try:
        async with conn.transaction():
            for update in updates:
                await conn.execute("""
                    UPDATE данные_по_атриб
                    SET данные = $1
                    WHERE id_данных = $2
                """, update["данные"], update["id_данных"])
    finally:
        await conn.close()


# ============================================================
# Синхронные версии (для фоновых executor-задач)
# ============================================================

def get_pending_attributes_sync(work_id: int) -> list[dict]:
    """Синхронная версия get_pending_attributes_for_work."""
    conn = get_sync_connection()
    try:
        with conn.cursor(cursor_factory=RealDictCursor) as cur:
            cur.execute("""
                SELECT 
                    д.id_данных,
                    д.id_работы,
                    д.данные AS текущее_значение,
                    а.id_атрибута,
                    а.название,
                    а.настройки
                FROM данные_по_атриб д
                JOIN атрибут_учреждения ау ON д.id_структуры = ау.id_структуры
                JOIN атрибут а ON ау.id_атрибута = а.id_атрибута
                WHERE д.id_работы = %s
                  AND д.данные = 'Ожидание поиска...'
                ORDER BY а.id_атрибута
            """, (work_id,))

            result = []
            for row in cur.fetchall():
                settings = json.loads(row["настройки"]) if row["настройки"] else {}
                result.append({
                    "id_данных": row["id_данных"],
                    "id_работы": row["id_работы"],
                    "id_атрибута": row["id_атрибута"],
                    "название": row["название"],
                    "текущее_значение": row["текущее_значение"],
                    "query": settings.get("query", ""),
                    "keywords": settings.get("keywords", []),
                    "examples": settings.get("examples", ""),
                })
            return result
    finally:
        conn.close()


def save_attribute_data_sync(data_id: int, value: str):
    """Синхронное сохранение одного атрибута."""
    conn = get_sync_connection()
    try:
        with conn.cursor() as cur:
            cur.execute("UPDATE данные_по_атриб SET данные = %s WHERE id_данных = %s",
                       (value, data_id))
        conn.commit()
    finally:
        conn.close()


def save_multiple_attributes_sync(updates: list[dict]):
    """Синхронное массовое сохранение."""
    conn = get_sync_connection()
    try:
        with conn.cursor() as cur:
            for update in updates:
                cur.execute("UPDATE данные_по_атриб SET данные = %s WHERE id_данных = %s",
                           (update["данные"], update["id_данных"]))
        conn.commit()
    finally:
        conn.close()