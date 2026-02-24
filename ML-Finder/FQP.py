"""
Программа для извлечения динамических атрибутов из текста ВКР
Автор: [Ваше имя]
Для дипломной работы "Реестр ВКР АГТУ"
"""

import re
import pandas as pd
from typing import Dict, List, Optional, Tuple
import logging
from dataclasses import dataclass
from enum import Enum

# Настройка логирования
logging.basicConfig(level=logging.INFO, 
                   format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

@dataclass
class ExtractedValue:
    """Структура для хранения извлеченного значения"""
    value: str
    confidence: float  # от 0.0 до 1.0
    method: str  # 'rule', 'ml', 'llm'
    source_text: str  # фрагмент текста, откуда извлечено

class AttributeType(Enum):
    """Типы динамических атрибутов"""
    PROGRAMMING_LANGUAGE = "язык программирования"
    LIBRARIES = "используемые библиотеки"
    DRAWINGS_COUNT = "количество чертежей"
    CODE_VOLUME = "объем кода"
    RESEARCH_METHOD = "методология исследования"
    SOFTWARE = "используемое программное обеспечение"
    GROUP = "группа"

class VKRExtractor:
    """Основной класс для извлечения атрибутов из ВКР"""
    
    def __init__(self):
        """Инициализация с загрузкой шаблонов и моделей"""
        self.patterns = self._load_patterns()
        self.speciality_contexts = self._load_contexts()
        
    def _load_patterns(self) -> Dict[str, List[str]]:
        """Загрузка шаблонов регулярных выражений для разных атрибутов"""
        return {
            AttributeType.PROGRAMMING_LANGUAGE.value: [
                r'(?:язык[аи]?\s+программирования|ЯП)[\s:]+([А-Яа-яA-Za-z\+#\d\.]+[^\s.,;!?])',
                r'(?:реализован[аоы]?|разработан[аоы]?|написан[аоы]?)\s+(?:на\s+)?языке?\s+([А-Яа-яA-Za-z\+#]+)',
                r'(?:использован[аоы]?\s+)?язык\s+([А-Яа-яA-Za-z\+#]+)(?:\s+для\s+программирования)?',
                r'\b(Python|Java|JavaScript|C\+\+|C#|PHP|Ruby|Go|Swift|Kotlin)\b',
            ],
            
            AttributeType.LIBRARIES.value: [
                r'(?:библиотек[аи]|library|framework)[\s:]+([^.,;\n]{5,100})',
                r'использован[аоы]?\s+(?:следующие\s+)?библиотеки[^.:\n]*[:.]\s*([^.]{5,200})',
                r'\b(?:библиотек[аи]\s+)?(Django|React|Vue\.js|Angular|NumPy|Pandas|TensorFlow|PyTorch|Spring|Hibernate)\b',
            ],
            
            AttributeType.DRAWINGS_COUNT.value: [
                r'(?:чертеж[аей]|лист[аов]|рисун[ков]+)[\s:]*(\d+)',
                r'графическ[аяой]\s+часть\s*(?:состоит из|включает|содержит)[^.\d]*(\d+)',
                r'приложени[ея]\s+\d+\s*[-–]\s*(\d+)\s+чертеж',
            ],
            
            AttributeType.CODE_VOLUME.value: [
                r'(?:объем|количество)\s+код[ау]\s*[:.]?\s*(\d+\s*(?:строк[и]?|SLOC|LOC))',
                r'\b(\d+\s*строк\s+код[ау])\b',
                r'написано\s+(\d+\s*строк\s+(?:код[ау]|программного кода))',
            ],
            
            AttributeType.SOFTWARE.value: [
                r'программн[а-я]+\s+обеспечени[ея][^.:\n]*[:.]\s*([^.]{5,150})',
                r'(?:использован[аоы]?|применен[аоы]?)\s+ПО[^.:\n]*[:.]\s*([^.]{5,150})',
                r'\b(?:систем[ау]\s+)?(Windows|Linux|Ubuntu|macOS|Docker|PostgreSQL|MySQL|MongoDB|Git)\b',
            ],
            
            AttributeType.GROUP.value: [
                r'Работа выполнена обучающ[имсяей] группы '
            ]
        }
    
    def _load_contexts(self) -> Dict[str, List[str]]:
        """Контексты для разных специальностей"""
        return {
            "ИИТиК": [
                AttributeType.GROUP.value,
                AttributeType.PROGRAMMING_LANGUAGE.value,
                AttributeType.LIBRARIES.value,
                AttributeType.CODE_VOLUME.value,
                AttributeType.SOFTWARE.value
            ],
            "Градостроительство": [
                AttributeType.GROUP.value,
                AttributeType.DRAWINGS_COUNT.value
            ],
            "Экономика": [
                AttributeType.GROUP.value
            ]
        }
    
    def extract_from_text(self, text: str, speciality: str = "ИИТиК") -> Dict[str, ExtractedValue]:
        """
        Основной метод извлечения атрибутов из текста
        
        Args:
            text: Текст ВКР
            speciality: Специальность (для определения нужных атрибутов)
            
        Returns:
            Словарь с извлеченными значениями
        """
        logger.info(f"Начинаем извлечение для специальности: {speciality}")
        
        # 1. Определяем какие атрибуты искать для этой специальности
        attributes_to_find = self.speciality_contexts.get(speciality, [])
        logger.info(f"Ищем атрибуты: {attributes_to_find}")
        
        # 2. Предобработка текста
        cleaned_text = self._preprocess_text(text)
        
        # 3. Извлечение каждого атрибута
        results = {}
        for attribute in attributes_to_find:
            logger.info(f"Извлекаем: {attribute}")
            extracted = self._extract_attribute(cleaned_text, attribute)
            if extracted:
                results[attribute] = extracted
        
        return results
    
    def _preprocess_text(self, text: str) -> str:
        """Очистка и нормализация текста"""
        # Удаляем лишние пробелы и переносы
        text = re.sub(r'\s+', ' ', text)
        # Нормализуем кавычки
        text = text.replace('"', '"').replace('«', '"').replace('»', '"')
        # Приводим к единому регистру для поиска
        return text.lower()
    
    def _extract_attribute(self, text: str, attribute: str) -> Optional[ExtractedValue]:
        """Извлечение конкретного атрибута"""
        
        # Метод 1: Поиск по шаблонам (правилам)
        rule_result = self._extract_by_rules(text, attribute)
        
        # Метод 2: Если правила не нашли, пробуем ML/статистические методы
        if not rule_result or rule_result.confidence < 0.5:
            # Здесь можно добавить вызов ML-модели
            ml_result = self._extract_by_statistics(text, attribute)
            if ml_result and ml_result.confidence > rule_result.confidence:
                return ml_result
        
        return rule_result
    
    def _extract_by_rules(self, text: str, attribute: str) -> Optional[ExtractedValue]:
        """Извлечение по регулярным выражениям"""
        patterns = self.patterns.get(attribute, [])
        
        all_matches = []
        for pattern in patterns:
            matches = re.finditer(pattern, text, re.IGNORECASE)
            for match in matches:
                value = match.group(1) if match.groups() else match.group(0)
                # Вычисляем "уверенность" на основе позиции в тексте
                # (ранние упоминания обычно важнее)
                position_score = 1.0 - (match.start() / len(text))
                confidence = 0.7 + (position_score * 0.3)  # 0.7-1.0
                
                all_matches.append({
                    'value': value.strip(),
                    'confidence': min(confidence, 0.95),
                    'source': text[max(0, match.start()-50):match.end()+50]
                })
        
        if not all_matches:
            return None
        
        # Выбираем лучшее совпадение (по уверенности)
        best_match = max(all_matches, key=lambda x: x['confidence'])
        
        # Если найдено много совпадений, можно их сгруппировать
        if len(all_matches) > 1:
            # Группируем похожие значения
            values = [m['value'] for m in all_matches]
            best_match['value'] = self._merge_values(values)
            # Повышаем уверенность при множественных подтверждениях
            best_match['confidence'] = min(0.98, best_match['confidence'] + 0.1)
        
        return ExtractedValue(
            value=best_match['value'],
            confidence=best_match['confidence'],
            method='rule',
            source_text=best_match['source']
        )
    
    def _extract_by_statistics(self, text: str, attribute: str) -> Optional[ExtractedValue]:
        """Статистические методы извлечения (упрощенный ML)"""
        # Для демонстрации - ищем ключевые слова в зависимости от атрибута
        
        keyword_dict = {
            "язык программирования": {
                'python': 0.9, 'java': 0.8, 'c++': 0.85, 'javascript': 0.75,
                'c#': 0.8, 'php': 0.7, 'ruby': 0.6, 'go': 0.7, 'swift': 0.65
            },
            "используемые библиотеки": {
                'django': 0.85, 'react': 0.8, 'vue': 0.75, 'angular': 0.7,
                'numpy': 0.9, 'pandas': 0.85, 'tensorflow': 0.8, 'pytorch': 0.8
            }
        }
        
        if attribute not in keyword_dict:
            return None
        
        found_keywords = []
        for keyword, base_confidence in keyword_dict[attribute].items():
            if keyword in text.lower():
                # Считаем частоту упоминания
                count = text.lower().count(keyword)
                frequency_boost = min(0.2, count * 0.05)
                confidence = base_confidence + frequency_boost
                
                found_keywords.append({
                    'keyword': keyword,
                    'confidence': confidence,
                    'count': count
                })
        
        if not found_keywords:
            return None
        
        # Выбираем наиболее частый и уверенный
        best = max(found_keywords, key=lambda x: (x['confidence'], x['count']))
        
        return ExtractedValue(
            value=best['keyword'],
            confidence=best['confidence'],
            method='statistical',
            source_text=f"Упоминается {best['count']} раз"
        )
    
    def _merge_values(self, values: List[str]) -> str:
        """Объединение нескольких найденных значений"""
        # Простейшая логика - берем самое длинное (обычно самое полное)
        return max(values, key=len)
    
    def save_to_csv(self, results: Dict[str, ExtractedValue], filename: str = "extracted_attributes.csv"):
        """Сохранение результатов в CSV"""
        data = []
        for attribute, extracted in results.items():
            data.append({
                'Атрибут': attribute,
                'Значение': extracted.value,
                'Уверенность': f"{extracted.confidence:.2%}",
                'Метод': extracted.method,
                'Источник': extracted.source_text[:100] + '...' if len(extracted.source_text) > 100 else extracted.source_text
            })
        
        df = pd.DataFrame(data)
        df.to_csv(filename, index=False, encoding='utf-8-sig')
        logger.info(f"Результаты сохранены в {filename}")

# ============================================================================
# Вспомогательные функции для работы с файлами
# ============================================================================

def read_docx(filepath: str) -> str:
    """Чтение текста из DOCX файла"""
    try:
        from docx import Document
        doc = Document(filepath)
        return "\n".join([paragraph.text for paragraph in doc.paragraphs])
    except ImportError:
        logger.warning("python-docx не установлен. Используйте: pip install python-docx")
        return ""
    except Exception as e:
        logger.error(f"Ошибка чтения DOCX: {e}")
        return ""

def read_pdf(filepath: str) -> str:
    """Чтение текста из PDF файла"""
    try:
        import pdfplumber
        text = ""
        with pdfplumber.open(filepath) as pdf:
            for page in pdf.pages:
                text += page.extract_text() + "\n"
        return text
    except ImportError:
        logger.warning("pdfplumber не установлен. Используйте: pip install pdfplumber")
        return ""
    except Exception as e:
        logger.error(f"Ошибка чтения PDF: {e}")
        return ""

# ============================================================================
# Веб-интерфейс (опционально)
# ============================================================================

def create_web_app(extractor: VKRExtractor):
    """Создание веб-интерфейса на FastAPI"""
    from fastapi import FastAPI, UploadFile, File, Form
    from fastapi.responses import HTMLResponse
    import json
    
    app = FastAPI(title="VKR Attribute Extractor API")
    
    @app.get("/", response_class=HTMLResponse)
    async def home():
        return """
        <html>
            <body>
                <h1>Извлечение атрибутов из ВКР</h1>
                <form action="/extract" method="post" enctype="multipart/form-data">
                    <label>Файл ВКР:</label><br>
                    <input type="file" name="file" accept=".docx,.pdf"><br><br>
                    <label>Специальность:</label><br>
                    <select name="speciality">
                        <option value="ИИТиК">ИИТиК</option>
                        <option value="Градостроительство">Градостроительство</option>
                        <option value="Экономика">Экономика</option>
                    </select><br><br>
                    <input type="submit" value="Извлечь атрибуты">
                </form>
            </body>
        </html>
        """
    
    @app.post("/extract")
    async def extract_attributes(
        file: UploadFile = File(...),
        speciality: str = Form("ИИТиК")
    ):
        """API endpoint для извлечения атрибутов"""
        # Чтение файла
        content = await file.read()
        
        # Сохраняем временно
        import tempfile
        import os
        
        with tempfile.NamedTemporaryFile(delete=False, suffix=os.path.splitext(file.filename)[1]) as tmp:
            tmp.write(content)
            tmp_path = tmp.name
        
        # Определяем тип файла и читаем
        if file.filename.lower().endswith('.docx'):
            text = read_docx(tmp_path)
        elif file.filename.lower().endswith('.pdf'):
            text = read_pdf(tmp_path)
        else:
            return {"error": "Неподдерживаемый формат файла"}
        
        # Удаляем временный файл
        os.unlink(tmp_path)
        
        # Извлекаем атрибуты
        results = extractor.extract_from_text(text, speciality)
        
        # Преобразуем для JSON
        json_results = {}
        for attr, extracted in results.items():
            json_results[attr] = {
                "value": extracted.value,
                "confidence": extracted.confidence,
                "method": extracted.method
            }
        
        return {
            "filename": file.filename,
            "speciality": speciality,
            "results": json_results
        }
    
    return app

# ============================================================================
# Основная функция для демонстрации
# ============================================================================

def main():
    """Основная демонстрационная функция"""
    print("=" * 60)
    print("ПРОГРАММА ИЗВЛЕЧЕНИЯ АТРИБУТОВ ИЗ ВКР")
    print("Для дипломной работы 'Реестр ВКР АГТУ'")
    print("=" * 60)
    
    # Создаем экстрактор
    extractor = VKRExtractor()
    
    # Пример 1: Демонстрация на тестовом тексте
    print("\n1. Тест на примере текста:")
    
    test_text = """
    Данная выпускная квалификационная работа посвящена разработке 
    информационной системы для учета ВКР в АГТУ. 
    Система реализована на языке программирования Python с использованием 
    библиотек Django и React. Объем написанного кода составляет около 1500 строк.
    Для управления версиями использована система Git.
    """
    
    print(f"Текст: {test_text}")
    
    results = extractor.extract_from_text(test_text, "ИИТиК")
    
    print("\nИзвлеченные атрибуты:")
    for attr, extracted in results.items():
        print(f"  • {attr}: {extracted.value} (уверенность: {extracted.confidence:.0%})")
    
    # Пример 2: Сохранение в CSV
    extractor.save_to_csv(results, "test_results.csv")
    print(f"\nРезультаты сохранены в test_results.csv")
    
    # Пример 3: Работа с реальным файлом (если есть)
    path_example = "D:\\учёба\\4 курс\\1 семестр\\Курсовая\\Примеры\\Казанский_Пояснительная_записка.docx"
    import os
    if os.path.exists(path_example):
        print(f"\n2. Обработка реального файла {path_example}:")
        text = read_docx(path_example)
        print(text[:1500])
        if text:
            results = extractor.extract_from_text(text, "ИИТиК")
            for attr, extracted in results.items():
                print(f"  • {attr}: {extracted.value}")
    
    # # Запуск веб-сервера (опционально)
    # run_web = input("\nЗапустить веб-интерфейс? (y/n): ")
    # if run_web.lower() == 'y':
    #     import uvicorn
    #     app = create_web_app(extractor)
    #     print("Сервер запущен: http://localhost:8000")
    #     print("Нажмите Ctrl+C для остановки")
    #     uvicorn.run(app, host="0.0.0.0", port=8000)

if __name__ == "__main__":
    main()