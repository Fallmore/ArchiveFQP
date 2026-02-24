"""
Программа для извлечения структурированных данных из титульных листов ВКР
Использует библиотеку Natasha для NER (Named Entity Recognition)
Автор: [Ваше имя]
Для дипломной работы "Реестр ВКР АГТУ"
"""

import re
import pandas as pd
from typing import Dict, List, Optional, Any
from dataclasses import dataclass
import logging
from enum import Enum

# Импортируем Natasha
from natasha import (
    Segmenter, MorphVocab,
    NewsEmbedding, NewsMorphTagger,
    NewsSyntaxParser, NewsNERTagger,
    Doc, NamesExtractor
)
from natasha.extractors import Extractor

# Настройка логирования
logging.basicConfig(level=logging.INFO, 
                   format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)

@dataclass
class ExtractedField:
    """Структура для хранения извлеченного поля"""
    value: str
    confidence: float  # 0.0-1.0
    method: str  # 'ner', 'regex', 'context'
    source: str  # исходный текст

class TitlePageExtractor:
    """Класс для извлечения данных из титульных листов ВКР"""
    
    def __init__(self, university="АГТУ"):
        """Инициализация Natasha и загрузка шаблонов"""
        self.university = university
        
        # Инициализация компонентов Natasha
        self.segmenter = Segmenter()
        self.morph_vocab = MorphVocab()
        self.emb = NewsEmbedding()
        self.morph_tagger = NewsMorphTagger(self.emb)
        self.syntax_parser = NewsSyntaxParser(self.emb)
        self.ner_tagger = NewsNERTagger(self.emb)
        self.names_extractor = NamesExtractor(self.morph_vocab)
        
        # Загрузка словарей и шаблонов
        self.degree_patterns = self._load_degree_patterns()
        self.position_patterns = self._load_position_patterns()
        self.education_patterns = self._load_education_patterns()
        
    def _load_degree_patterns(self) -> Dict[str, List[str]]:
        """Шаблоны для ученых степеней и званий"""
        return {
            'degree': [
                r'к\.\s*т\.\s*н\.',  # к.т.н.
                r'к\.\s*ф\.-м\.\s*н\.',  # к.ф.-м.н.
                r'к\.\s*и\.\s*н\.',  # к.и.н.
                r'д\.\s*т\.\s*н\.',  # д.т.н.
                r'д\.\s*ф\.-м\.\s*н\.',  # д.ф.-м.н.
                r'кандидат\s+[а-я]+\s+наук',
                r'доктор\s+[а-я]+\s+наук',
            ],
            'title': [
                r'доцент\w*',
                r'профессор\w*',
                r'старший\s+преподаватель',
                r'ассистент\w*',
            ]
        }
    
    def _load_position_patterns(self) -> Dict[str, List[str]]:
        """Шаблоны для должностей"""
        return {
            'position': [
                r'зав\.\s*кафедр\w+',
                r'декан\w*',
                r'директор\w*',
                r'заместитель\w*',
                r'начальник\w*',
            ]
        }
    
    def _load_education_patterns(self) -> Dict[str, List[str]]:
        """Шаблоны для уровней образования и стандартов"""
        return {
            'education_level': [
                r'бакалавр\w*',
                r'магистр\w*',
                r'специалист\w*',
                r'аспирант\w*',
            ],
            'standard': [
                r'ГОС\s*ВПО\s*\(\s*2\s*\)',
                r'ФГОС\s*ВПО\s*\(\s*3\s*\)',
                r'ФГОС\s*ВО\s*\(\s*3\+\s*\)',
                r'ГОС\s*ВПО',
                r'ФГОС\s*ВПО',
                r'ФГОС\s*ВО',
            ],
            'form': [
                r'очн\w*\s+форм\w*',
                r'заочн\w*\s+форм\w*',
                r'очно-заочн\w*',
            ]
        }
    
    def extract_all_fields(self, text: str) -> Dict[str, ExtractedField]:
        """
        Основной метод извлечения всех полей
        
        Args:
            text: Текст титульного листа
            
        Returns:
            Словарь со всеми извлеченными полями
        """
        logger.info("Начинаем извлечение полей из титульного листа")
        
        # Предобработка текста
        cleaned_text = self._preprocess_text(text)
        
        # Создаем документ Natasha
        doc = Doc(cleaned_text)
        doc.segment(self.segmenter)
        doc.tag_morph(self.morph_tagger)
        doc.parse_syntax(self.syntax_parser)
        doc.tag_ner(self.ner_tagger)
        
        # Извлекаем каждое поле
        results = {}
        
        # 1. Тема ВКР
        results['Тема ВКР'] = self._extract_topic(cleaned_text, doc)
        
        # 2. ФИО студента
        results['ФИО студента'] = self._extract_student_name(cleaned_text, doc)
        
        # 3. Руководитель
        supervisor_data = self._extract_supervisor(cleaned_text, doc)
        results['Ученая степень, должность руководителя'] = supervisor_data['degree_position']
        results['ФИО руководителя'] = supervisor_data['name']
        
        # 4. Консультанты
        results['Консультанты'] = self._extract_consultants(cleaned_text, doc)
        
        # 5. Рецензенты
        results['Рецензенты'] = self._extract_reviewers(cleaned_text, doc)
        
        # 6. Стандарт
        results['Стандарт'] = self._extract_standard(cleaned_text)
        
        # 7. УГСН
        results['УГСН'] = self._extract_ugsn(cleaned_text)
        
        # 8. Направление подготовки
        results['Направление подготовки'] = self._extract_direction(cleaned_text)
        
        # 9. Профиль
        results['Профиль'] = self._extract_profile(cleaned_text)
        
        # 10. Специализация
        results['Специализация'] = self._extract_specialization(cleaned_text)
        
        # 11. Специальность
        results['Специальность'] = self._extract_speciality(cleaned_text)
        
        # 12. Магистерская программа
        results['Магистерская программа'] = self._extract_masters_program(cleaned_text)
        
        # 13. Уровень образования
        results['Уровень образования'] = self._extract_education_level(cleaned_text)
        
        # 14. Факультет/Институт
        results['Факультет'] = self._extract_faculty(cleaned_text)
        results['Институт'] = self._extract_institute(cleaned_text)
        
        # 15. Выпускающая кафедра
        results['Выпускающая кафедра'] = self._extract_department(cleaned_text)
        
        # 16. Год выпуска
        results['Год выпуска'] = self._extract_year(cleaned_text, doc)
        
        # 17. Форма обучения
        results['Форма обучения'] = self._extract_education_form(cleaned_text)
        
        # 18. Аннотация
        results['Аннотация'] = self._extract_annotation(cleaned_text)
        
        # 19. Объем ВКР
        results['Объем ВКР'] = self._extract_volume(cleaned_text)
        
        return results
    
    def _preprocess_text(self, text: str) -> str:
        """Очистка и нормализация текста"""
        # Удаляем лишние пробелы и переносы
        text = re.sub(r'\s+', ' ', text)
        # Сохраняем заглавные буквы для имен
        return text
    
    def _extract_topic(self, text: str, doc: Doc) -> ExtractedField:
        """Извлечение темы ВКР"""
        # Ищем текст в кавычках после ключевых слов
        patterns = [
            r'ТЕМА[^:]*:\s*["«]([^"»]+)["»]',
            r'Название[^:]*:\s*["«]([^"»]+)["»]',
            r'Тема\s+работы[^:]*:\s*["«]?([^"\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.95,
                    method='regex',
                    source=match.group(0)
                )
        
        # Если не нашли по шаблону, ищем самый длинный текст в кавычках
        quotes = re.findall(r'["«]([^"»]+)["»]', text)
        if quotes:
            # Берем самую длинную строку в кавычках (скорее всего это тема)
            longest = max(quotes, key=len)
            if len(longest) > 20:  # Минимальная длина для темы
                return ExtractedField(
                    value=longest.strip(),
                    confidence=0.7,
                    method='quotes',
                    source=f'"{longest}"'
                )
        
        return ExtractedField(value='Не найдено', confidence=0.0, method='none', source='')
    
    def _extract_student_name(self, text: str, doc: Doc) -> ExtractedField:
        """Извлечение ФИО студента с помощью Natasha"""
        # 1. Ищем по ключевым словам
        patterns = [
            r'Студент[^:]*:\s*([А-ЯЁ][а-яё]+(?:\s+[А-ЯЁ][а-яё]+){1,2})',
            r'Выполнил[^:]*:\s*([А-ЯЁ][а-яё]+(?:\s+[А-ЯЁ][а-яё]+){1,2})',
            r'Автор[^:]*:\s*([А-ЯЁ][а-яё]+(?:\s+[А-ЯЁ][а-яё]+){1,2})',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.9,
                    method='regex',
                    source=match.group(0)
                )
        
        # 2. Используем Natasha для извлечения имен (PER)
        names = []
        for span in doc.spans:
            if span.type == 'PER':
                # Нормализуем имя
                span.normalize(self.morph_vocab)
                names.append(span.normal)
        
        # 3. Выбираем первое найденное имя (скорее всего это студент)
        if names:
            return ExtractedField(
                value=names[0],
                confidence=0.8,
                method='ner',
                source=', '.join(names)
            )
        
        return ExtractedField(value='Не найдено', confidence=0.0, method='none', source='')
    
    def _extract_supervisor(self, text: str, doc: Doc) -> Dict[str, ExtractedField]:
        """Извлечение данных о руководителе"""
        result = {
            'name': ExtractedField(value='Не найдено', confidence=0.0, method='none', source=''),
            'degree_position': ExtractedField(value='Не найдено', confidence=0.0, method='none', source='')
        }
        
        # Ищем блок с руководителем
        supervisor_section = None
        lines = text.split('\n')
        for i, line in enumerate(lines):
            if 'руководитель' in line.lower():
                # Берем текущую и следующую строку
                supervisor_section = line
                if i + 1 < len(lines):
                    supervisor_section += ' ' + lines[i + 1]
                break
        
        if supervisor_section:
            # Извлекаем ФИО
            name_patterns = [
                r'[А-ЯЁ][а-яё]+(?:\s+[А-ЯЁ][а-яё]+){1,2}',
            ]
            
            for pattern in name_patterns:
                matches = re.findall(pattern, supervisor_section)
                if matches:
                    # Предполагаем, что первое ФИО в разделе - руководитель
                    result['name'] = ExtractedField(
                        value=matches[0],
                        confidence=0.85,
                        method='regex',
                        source=supervisor_section
                    )
                    break
            
            # Извлекаем степень и должность
            degree_found = []
            position_found = []
            
            for degree_pattern in self.degree_patterns['degree']:
                if re.search(degree_pattern, supervisor_section, re.IGNORECASE):
                    degree_found.append(re.search(degree_pattern, supervisor_section, re.IGNORECASE).group(0))
            
            for position_pattern in self.position_patterns['position']:
                if re.search(position_pattern, supervisor_section, re.IGNORECASE):
                    position_found.append(re.search(position_pattern, supervisor_section, re.IGNORECASE).group(0))
            
            if degree_found or position_found:
                result['degree_position'] = ExtractedField(
                    value=', '.join(degree_found + position_found),
                    confidence=0.8,
                    method='regex',
                    source=supervisor_section
                )
        
        return result
    
    def _extract_consultants(self, text: str, doc: Doc) -> ExtractedField:
        """Извлечение данных о консультантах"""
        # Ищем раздел с консультантами
        consultant_text = ''
        lines = text.split('\n')
        
        for i, line in enumerate(lines):
            if 'консультант' in line.lower():
                # Собираем несколько строк после ключевого слова
                consultant_text = line
                for j in range(1, 4):  # Следующие 3 строки
                    if i + j < len(lines):
                        consultant_text += ' ' + lines[i + j]
                break
        
        if consultant_text:
            # Ищем ФИО в тексте
            names = re.findall(r'[А-ЯЁ][а-яё]+(?:\s+[А-ЯЁ][а-яё]+){1,2}', consultant_text)
            
            if names:
                return ExtractedField(
                    value='; '.join(names),
                    confidence=0.7,
                    method='regex',
                    source=consultant_text
                )
        
        return ExtractedField(value='Не указаны', confidence=0.9, method='none', source='')
    
    def _extract_reviewers(self, text: str, doc: Doc) -> ExtractedField:
        """Извлечение данных о рецензентах"""
        # Аналогично консультантам
        return self._extract_consultants(text, doc)  # Для демо используем ту же логику
    
    def _extract_standard(self, text: str) -> ExtractedField:
        """Извлечение стандарта"""
        for pattern in self.education_patterns['standard']:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(0),
                    confidence=0.9,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указан', confidence=0.8, method='none', source='')
    
    def _extract_ugsn(self, text: str) -> ExtractedField:
        """Извлечение УГСН"""
        # Ищем код и название УГСН
        patterns = [
            r'УГСН[^:]*:\s*([^\n]+)',
            r'\b\d{2}\.\d{2}\.\d{2}[^\n]*',  # Код вида 01.02.03
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(0).strip(),
                    confidence=0.8,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указан', confidence=0.8, method='none', source='')
    
    def _extract_direction(self, text: str) -> ExtractedField:
        """Извлечение направления подготовки"""
        patterns = [
            r'Направление[^:]*подготовки[^:]*:\s*([^\n]+)',
            r'Направление[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.85,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указано', confidence=0.8, method='none', source='')
    
    def _extract_profile(self, text: str) -> ExtractedField:
        """Извлечение профиля"""
        patterns = [
            r'Профиль[^:]*:\s*([^\n]+)',
            r'Профиль\s+подготовки[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.8,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указан', confidence=0.9, method='none', source='')
    
    def _extract_specialization(self, text: str) -> ExtractedField:
        """Извлечение специализации"""
        patterns = [
            r'Специализация[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.8,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указана', confidence=0.9, method='none', source='')
    
    def _extract_speciality(self, text: str) -> ExtractedField:
        """Извлечение специальности"""
        patterns = [
            r'Специальность[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.85,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указана', confidence=0.8, method='none', source='')
    
    def _extract_masters_program(self, text: str) -> ExtractedField:
        """Извлечение магистерской программы"""
        patterns = [
            r'Магистерская\s+программа[^:]*:\s*([^\n]+)',
            r'Программа[^:]*магистратур[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.85,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указана', confidence=0.9, method='none', source='')
    
    def _extract_education_level(self, text: str) -> ExtractedField:
        """Извлечение уровня образования"""
        for pattern in self.education_patterns['education_level']:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(0),
                    confidence=0.9,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указан', confidence=0.8, method='none', source='')
    
    def _extract_faculty(self, text: str) -> ExtractedField:
        """Извлечение факультета"""
        patterns = [
            r'Факультет[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.85,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указан', confidence=0.8, method='none', source='')
    
    def _extract_institute(self, text: str) -> ExtractedField:
        """Извлечение института"""
        patterns = [
            r'Институт[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.85,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указан', confidence=0.9, method='none', source='')
    
    def _extract_department(self, text: str) -> ExtractedField:
        """Извлечение выпускающей кафедры"""
        patterns = [
            r'Кафедр[^:]*:\s*([^\n]+)',
            r'Выпускающая\s+кафедра[^:]*:\s*([^\n]+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1).strip(),
                    confidence=0.9,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указана', confidence=0.8, method='none', source='')
    
    def _extract_year(self, text: str, doc: Doc) -> ExtractedField:
        """Извлечение года выпуска"""
        # Ищем год в тексте (4 цифры)
        year_pattern = r'\b(19|20)\d{2}\b'
        matches = re.findall(year_pattern, text)
        
        if matches:
            # Берем последний найденный год (скорее всего актуальный)
            return ExtractedField(
                value=matches[-1],
                confidence=0.9,
                method='regex',
                source=', '.join(matches)
            )
        
        return ExtractedField(value='Не указан', confidence=0.8, method='none', source='')
    
    def _extract_education_form(self, text: str) -> ExtractedField:
        """Извлечение формы обучения"""
        for pattern in self.education_patterns['form']:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(0),
                    confidence=0.9,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указана', confidence=0.8, method='none', source='')
    
    def _extract_annotation(self, text: str) -> ExtractedField:
        """Извлечение аннотации"""
        # Ищем текст после слова "Аннотация"
        pattern = r'Аннотация[^:]*[:.]\s*([^\n]{10,500})'
        match = re.search(pattern, text, re.IGNORECASE)
        
        if match:
            return ExtractedField(
                value=match.group(1).strip(),
                confidence=0.8,
                method='regex',
                source=match.group(0)[:100] + '...'
            )
        
        return ExtractedField(value='Не указана', confidence=0.9, method='none', source='')
    
    def _extract_volume(self, text: str) -> ExtractedField:
        """Извлечение объема ВКР (количество страниц)"""
        patterns = [
            r'\b(\d+)\s*стр\.',
            r'\b(\d+)\s*страниц',
            r'объем[^:]*:\s*(\d+)',
        ]
        
        for pattern in patterns:
            match = re.search(pattern, text, re.IGNORECASE)
            if match:
                return ExtractedField(
                    value=match.group(1),
                    confidence=0.9,
                    method='regex',
                    source=match.group(0)
                )
        
        return ExtractedField(value='Не указан', confidence=0.8, method='none', source='')
    
    def save_to_csv(self, results: Dict[str, ExtractedField], filename: str = "title_page_data.csv"):
        """Сохранение результатов в Excel"""
        data = []
        for field_name, extracted in results.items():
            data.append({
                'Поле': field_name,
                'Значение': extracted.value,
                'Уверенность': f"{extracted.confidence:.0%}",
                'Метод извлечения': extracted.method,
                'Источник': extracted.source[:150] + '...' if len(extracted.source) > 150 else extracted.source
            })
        
        df = pd.DataFrame(data)
        df.to_csv(filename, index=False)
        logger.info(f"Результаты сохранены в {filename}")
        return df

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
# Демонстрационная функция
# ============================================================================

def main():
    """Основная демонстрационная функция"""
    print("=" * 70)
    print("ПРОГРАММА ДЛЯ ИЗВЛЕЧЕНИЯ ДАННЫХ ИЗ ТИТУЛЬНЫХ ЛИСТОВ ВКР")
    print("Использует библиотеку Natasha для распознавания именованных сущностей")
    print("=" * 70)
    
    # Создаем экстрактор
    extractor = TitlePageExtractor(university="АГТУ")
    
    # Пример титульного листа (реальный пример)
    example_text = """
    МИНИСТЕРСТВО НАУКИ И ВЫСШЕГО ОБРАЗОВАНИЯ РОССИЙСКОЙ ФЕДЕРАЦИИ
    Федеральное государственное бюджетное образовательное учреждение
    высшего образования
    «АСТРАХАНСКИЙ ГОСУДАРСТВЕННЫЙ ТЕХНИЧЕСКИЙ УНИВЕРСИТЕТ»
    
    ИНСТИТУТ ИНФОРМАЦИОННЫХ ТЕХНОЛОГИЙ И КОММУНИКАЦИЙ
    
    Кафедра «Информационные системы и технологии»
    
    ДОПУСТИТЬ К ЗАЩИТЕ
    Зав. кафедрой __________ /Иванов А.А./
    «___» __________ 2024 г.
    
    ВЫПУСКНАЯ КВАЛИФИКАЦИОННАЯ РАБОТА
    
    ТЕМА: «Разработка информационной системы "Реестр ВКР" для Астраханского государственного технического университета»
    
    Студент: Петров Иван Сергеевич
    
    Научный руководитель:
    к.т.н., доцент Сидоров Петр Васильевич
    
    Консультанты:
    по экономической части: ст. преподаватель Кузнецова М.И.
    по безопасности: доцент Васильев С.С.
    
    Нормоконтролер: 
    доцент Николаева О.П.
    
    Рецензент:
    д.т.н., профессор Козлов В.В.
    
    Направление подготовки: 09.03.02 Информационные системы и технологии
    Профиль: Информационные системы и технологии в бизнесе
    Уровень образования: бакалавриат
    Форма обучения: очная
    
    Астрахань – 2024
    """
    
    print("\nПример титульного листа ВКР:")
    print("-" * 50)
    print(example_text[:500] + "...")
    print("-" * 50)
    
    # Извлекаем данные
    print("\nИзвлеченные данные:")
    print("-" * 50)
    
    results = extractor.extract_all_fields(example_text)
    
    for field_name, extracted in results.items():
        if extracted.value != 'Не найдено' and extracted.value != 'Не указан':
            print(f"{field_name}: {extracted.value} (уверенность: {extracted.confidence:.0%})")
    
    # Сохраняем в Excel
    df = extractor.save_to_csv(results, "example_extraction.csv")
    
    print("\n" + "=" * 70)
    print("Краткая статистика:")
    print(f"Всего полей: {len(results)}")
    found = sum(1 for v in results.values() if v.confidence > 0.5)
    print(f"Успешно извлечено: {found}")
    
    # Демонстрация работы Natasha
    print("\nДемонстрация работы Natasha (распознавание имен):")
    print("-" * 50)
    
    doc = Doc(example_text)
    doc.segment(extractor.segmenter)
    doc.tag_morph(extractor.morph_tagger)
    doc.parse_syntax(extractor.syntax_parser)
    doc.tag_ner(extractor.ner_tagger)
    
    print("Распознанные сущности:")
    for span in doc.spans:
        span.normalize(extractor.morph_vocab)
        print(f"  • {span.normal} ({span.type})")
    
    # Тест с реальным файлом (если есть)
    import os
    test_files = ["D:\\учёба\\4 курс\\1 семестр\\Курсовая\\Примеры\\Казанский_Пояснительная_записка.docx", "title_page.pdf", "титульник.doc"]
    
    for file in test_files:
        if os.path.exists(file):
            print(f"\nОбработка файла: {file}")
            
            if file.endswith('.docx'):
                text = read_docx(file)
            elif file.endswith('.pdf'):
                text = read_pdf(file)
            else:
                continue
            
            if text:
                results = extractor.extract_all_fields(text)
                output_file = f"results_Казанский_Пояснительная_записка.csv"
                extractor.save_to_csv(results, output_file)
                print(f"  Результаты сохранены в {output_file}")

if __name__ == "__main__":
    main()