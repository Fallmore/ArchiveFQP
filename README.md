# Информационная система учета выпускных работ (ArchiveFQP)

[![.NET](https://img.shields.io/badge/.NET-10.0-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly%20%7C%20Server-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![Python](https://img.shields.io/badge/Python-3.12-3776AB?logo=python&logoColor=white)](https://www.python.org/)
[![FastAPI](https://img.shields.io/badge/FastAPI-009688?logo=fastapi&logoColor=white)](https://fastapi.tiangolo.com/)
[![Ollama](https://img.shields.io/badge/Ollama-llama3.2-000000?logo=ollama)](https://ollama.com/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

**ИС учета выпускных работ** — веб-приложение для централизованного учета, хранения, поиска и выдачи выпускных квалификационных работ (ВКР) в вузах. Разработано в рамках выпускной квалификационной работы для Астраханского государственного технического университета (АГТУ).

Проект решает проблему неэффективного ручного документооборота: единая база данных с **гибридным поиском** (семантический поиск на локальной нейросети + полнотекстовый BM25), автоматическим извлечением метаданных из титульных листов и контролем целостности файлов.

---

## Ключевые показатели

Расчеты по данным одной кафедры (60 работ в год) из пояснительной записки к проекту:

| Операция | Ручной процесс | В системе | Эффект |
| :--- | :---: | :---: | :---: |
| Архивирование работы | 5 минут | 10 секунд | **×30 быстрее** |
| Поиск работы | до 10 минут | 3 секунды | **×200 быстрее** |
| Выдача работы | до 1,5 дней | до 5 часов | в разы быстрее |
| Формирование отчета | 4 часа | 4 минуты | **×60 быстрее** |

Суммарная экономия трудоемкости — **более 1250 часов в год на одну кафедру**. Заявленные нефункциональные требования: время отклика ≤ 2 с при 1000 запросов, 50–100 одновременных пользователей, доступность 99,9% времени в год.

---

## Основные возможности

- **Централизованный реестр ВКР**: хранение работ с метаданными (студент, руководитель, год, институт, кафедра, направление, профиль и др.).
- **Гибридный поиск**: семантический поиск по содержанию работ (векторная база Chroma + эмбеддинги `all-minilm` через Ollama) в комбинации с полнотекстовым BM25, плюс многокритериальный поиск по полям и динамическим атрибутам.
- **Автоматическое извлечение данных**: при добавлении работы локальная нейросеть (`llama3.2`) считывает метаданные с титульных листов; для сканированных PDF подключается OCR (Tesseract, русский и английский языки).
- **Динамические атрибуты**: гибкая EAV-модель (Entity-Attribute-Value) — новые поля для работ добавляются в зависимости от института, кафедры или специальности без изменения схемы БД.
- **Безопасный просмотр**: показ PDF без скачивания, с водяными знаками и запретом копирования/печати.
- **Контроль целостности**: составной хеш (ЭЦП) файлов работы; отчет о верификации включает статистику валидных/невалидных ЭЦП.
- **Заявки на выдачу**: студенты и преподаватели подают заявления, сотрудники архива и зав. кафедрами обрабатывают их.
- **Ролевая модель**: разграничение прав для 7+ ролей (админ, студент, преподаватель, сотрудник архива, зав. кафедрой, ответственный института, делопроизводитель и др.), включая роли, привязанные к конкретной структуре вуза.
- **Интеграция с образовательным порталом**: OAuth2-авторизация через провайдер 3KL.
- **Уведомления по email**: автоматическая отправка писем через SMTP (уведомления о заявлениях и статусах).
- **Резервное копирование**: автоматические бэкапы БД утилитой `pg_dump` по расписанию с настройкой срока хранения и ручным запуском из панели администратора.
- **Генерация отчетов**: формирование отчетов в формате Word по результатам поиска и статистики.

---

## Архитектура

```
┌────────────────────┐     HTTP API      ┌──────────────────────┐     Ollama API    ┌─────────────┐
│  Blazor (.NET 10)  │ ────────────────► │   AiExtractor        │ ────────────────► │   Ollama    │
│  веб-приложение    │                   │   Python / FastAPI   │                   │  llama3.2,  │
└─────────┬──────────┘                   └──────────┬───────────┘                   │ all-minilm  │
          │ EF Core                                 │ asyncpg / LangChain           └─────────────┘
          ▼                                         ▼
┌────────────────────┐                   ┌──────────────────────┐
│    PostgreSQL 16   │ ◄───────────────  │  Chroma (векторная   │
│  (данные, файлы)   │                   │  БД, семантика)      │
└────────────────────┘                   └──────────────────────┘
```

- **Blazor-приложение** (`ArchiveFqp/`) — основной сервис: страницы, роли, заявления, отчеты, просмотр PDF, интеграция с OAuth и SMTP.
- **AiExtractor** (`AiExtractor/`) — отдельный Python-сервис извлечения данных из PDF: парсинг титульных листов, OCR сканов, нарезка на чанки и построение семантического индекса через LangChain + Chroma.
- **Ollama** — локальные модели `llama3.2` (генерация/извлечение данных) и `all-minilm` (эмбеддинги). Данные никуда не уходят за периметр — важно для вузовского документооборота.

---

## Технологический стек

| Компонент | Технология |
| :--- | :--- |
| **Frontend / UI** | [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) (.NET 10) |
| **Backend** | ASP.NET Core (C# 13) |
| **База данных** | [PostgreSQL 16](https://www.postgresql.org/) + Entity Framework Core |
| **AI-сервис** | Python 3.12, [FastAPI](https://fastapi.tiangolo.com/), asyncpg |
| **LLM-фреймворк** | [LangChain](https://www.langchain.com/) (`langchain-ollama`, `langchain-chroma`) |
| **Векторная БД** | [Chroma](https://www.trychroma.com/) (эмбеддинги `all-minilm` через Ollama) |
| **Полнотекстовый поиск** | BM25 (`rank_bm25`) в связке с семантическим поиском |
| **OCR** | Tesseract (русский + английский) для сканированных титульных листов |
| **Локальная LLM** | [Ollama](https://ollama.com/) (`llama3.2`) |
| **Деплой** | Docker Compose, nginx (опционально), systemd (Linux), pg_dump (бэкапы) |

---

## Примеры интерфейсов

| Галерея работ | Просмотр ПЗ | Добавление работ | Настройки кафедр |
| :---: | :---: | :---: | :---: |
| <img width="1586" height="778" alt="image" src="https://github.com/user-attachments/assets/82f00362-5d32-4b61-b417-ca0f4c4d3cd0" /> |  <img width="1584" height="776" alt="image" src="https://github.com/user-attachments/assets/2b736dd0-c278-492f-95e3-2318cadf1790" /> | <img width="850" height="669" alt="image" src="https://github.com/user-attachments/assets/54ea779d-c8d3-4740-a70c-51e5a7ee6591" /> | <img width="1167" height="716" alt="image" src="https://github.com/user-attachments/assets/9bc22e51-4ec8-47a8-8db6-62e81fec9d93" /> |

---

## Быстрый старт

### Способ 0. Docker Compose (самый простой)

Требуется [Docker](https://docs.docker.com/get-docker/) с Compose. Поднимаются PostgreSQL и Blazor-приложение; БД инициализируется из `backup.sql` автоматически.

По умолчанию приложение публикуется на **`https://localhost:8443`** (порт `8001` — HTTP, с редиректом на HTTPS). Порты задаются в `docker-compose.override.yml`.

1. **Положите `backup.sql` в корень репозитория.** В git он не входит; скачайте из [релизов](https://github.com/Fallmore/ArchiveFQP/releases) (`Utilities.zip`) или возьмите из своей БД (`pg_dump`).

2. **Создайте файл окружения** из шаблона:
   ```bash
   cp .env.example .env
   ```
3. **Заполните `.env`**: пароли БД, SMTP и OAuth-провайдер (3KL) — по желанию.

4. **Подготовьте HTTPS-сертификат.** Контейнеру нужен PFX-файл, иначе Kestrel откажется стартовать с ошибкой `Unable to configure HTTPS endpoint`:
   ```bash
   # создать/экспортировать dev-сертификат (пароль укажите свой)
   dotnet dev-certs https -ep certs/aspnetapp.pfx -p <пароль>
   ```
   Этот же пароль пропишите в `DEV_CERT_PASSWORD` в `.env`. Папка `certs/` входит в `.gitignore`, так как содержит закрытый ключ.

   *Для доверия сертификату в браузере на Windows/macOS выполните `dotnet dev-certs https --trust`. Для продакшна используйте реальный сертификат (см. `nginx.conf`), а не dev-сертификат.*

5. **Запустите**:
   ```bash
   docker compose up -d
   ```
6. **Проверьте состояние**:
   ```bash
   docker compose ps
   ```
7. *(Опционально)* Для запуска AI-сервиса и Ollama в контейнерах раскомментируйте соответствующие сервисы в `docker-compose.yml`.

> Для продакшна предусмотрен шаблон nginx с TLS: `nginx.conf` (проксирует на приложение; замените `your-domain.com` и укажите пути к сертификатам).

---

### Способ 1. Разработка (Windows / Linux)

Подходит для внесения изменений в код и отладки.

1. **Клонируйте репозиторий**:
   ```bash
   git clone https://github.com/Fallmore/ArchiveFQP.git
   cd ArchiveFQP
   ```
2. **Восстановите зависимости**:
   ```bash
   dotnet restore
   ```
3. **Подготовьте базу данных** (миграций EF в репозитории нет, БД создаётся из дампа):
   - Установите [PostgreSQL 16](https://www.postgresql.org/download/).
   - Создайте базу (например, `archive_fqp`) и восстановите её из `backup.sql` (в корне репозитория или в `Utilities.zip` из [релизов](https://github.com/Fallmore/ArchiveFQP/releases)):
     ```bash
     psql -U postgres -d archive_fqp -f backup.sql
     ```
4. **Настройте подключение** в `ArchiveFqp/ArchiveFqp/appsettings.json`, секция `ConnectionStrings:ArchiveFqpContext` (сервер, логин, пароль, имя БД).
5. **Запустите приложение**:
   - Откройте `ArchiveFQP.sln` в **Visual Studio** и нажмите `F5`;
   - **или** из командной строки:
     ```bash
     dotnet run --project ArchiveFqp
     ```
6. Откройте в браузере адрес из вывода команды (по умолчанию `http://localhost:5188` или `https://localhost:7038` — см. `Properties/launchSettings.json`).

---

### Способ 2. Релиз (Windows x64)

Быстрый запуск на Windows-сервере без Visual Studio.

1. Скачайте архив **`win-x64 debug.zip`** со страницы [релизов](https://github.com/Fallmore/ArchiveFQP/releases) и распакуйте (например, в `C:\ArchiveFQP`):
   - `publish/` — приложение;
   - `Utilities.zip` — `AiExtractor` и `backup.sql`.
2. Установите PostgreSQL 16 и восстановите базу из `backup.sql`.
3. В `publish/appsettings.json` укажите строку подключения к БД.
4. Запустите `publish/ArchiveFqp.exe`.

---

### Способ 3. Релиз (Linux x64) как системный сервис

1. Скачайте **`linux-x64 debug.zip`** с [страницы релизов](https://github.com/Fallmore/ArchiveFQP/releases), распакуйте на сервер:
   ```bash
   unzip "linux-x64 debug.zip" -d /home/userpc/ArchiveFQP/
   ```
2. Установите PostgreSQL, восстановите базу из `backup.sql` (внутри `Utilities.zip`).
3. В `/home/userpc/ArchiveFQP/publish/appsettings.json` укажите строку подключения к БД и адрес сервиса AiExtractor (если используется).
4. Создайте службу `/etc/systemd/system/archive-fqp.service`:
   ```ini
   [Unit]
   Description=ArchiveFQP Blazor Application
   After=network.target postgresql.service

   [Service]
   Type=simple
   WorkingDirectory=/home/userpc/ArchiveFQP/publish
   ExecStart=/usr/bin/dotnet /home/userpc/ArchiveFQP/publish/ArchiveFqp.dll
   Restart=on-failure
   RestartSec=10
   User=userpc
   Environment=DOTNET_ENVIRONMENT=Production

   [Install]
   WantedBy=multi-user.target
   ```
5. Включите автозапуск:
   ```bash
   sudo systemctl daemon-reload
   sudo systemctl enable archive-fqp.service
   sudo systemctl start archive-fqp.service
   sudo systemctl status archive-fqp.service
   ```

---

### Сервис извлечения данных (AiExtractor)

Python-сервис для парсинга титульных листов PDF, OCR сканов и построения семантического индекса. Может работать на Windows и Linux, отдельно от основного приложения.

1. Установите **Python 3.12** и [Ollama](https://ollama.com/download); скачайте модели:
   ```bash
   ollama pull llama3.2
   ollama pull all-minilm
   ```
2. Распакуйте папку `AiExtractor` (из `Utilities.zip`) и установите зависимости:
   ```bash
   pip install -r requirements.txt
   ```
   *Для OCR дополнительно требуется Tesseract с русскими и английскими языковыми пакетами.*
3. Настройте `AiExtractor/config.py`: подключение к PostgreSQL, адрес Ollama, модели по умолчанию.
4. Запустите:
   ```bash
   python api_server.py
   ```
   *На Linux рекомендуется оформить отдельной systemd-службой (`ai-extractor.service`) по аналогии с основным приложением.*

---

## Конфигурация

| Ключ (`appsettings.json` / `.env`) | Назначение |
| :--- | :--- |
| `ConnectionStrings:ArchiveFqpContext` / `DB_*` | Подключение к PostgreSQL |
| `AiExtractor:BaseUrl` / `AI_PORT` | Адрес Python-сервиса извлечения данных |
| `ThreeKl:*` / `THREEKL_*` | OAuth2-провайдер образовательного портала (clientId, endpoints) |
| `SmtpSettings:*` / `SMTP_*` | Исходящая почта (уведомления): сервер, порт, логин |
| `Kestrel:Certificates:Default:*` / `DEV_CERT_PASSWORD` | HTTPS-сертификат контейнера (путь к `certs/aspnetapp.pfx` и пароль к нему) |

Полный список переменных окружения — в шаблоне [`.env.example`](.env.example). Значения OAuth и SMTP выдаются администратором вуза/портала.

---

## Статус и ограничения

- Учебный проект, разработан в рамках ВКР; коммерческая эксплуатация требует доработки (хардeнинг, аудит безопасности, масштабирование).
- Все AI-компоненты работают локально — файлы ВКР не покидают инфраструктуру вуза.

---

## Лицензия

Проект распространяется по лицензии [MIT](LICENSE).

---

## Контакты

Почта: slava_samodurov@mail.ru

## Подробная документация

https://drive.google.com/drive/u/0/folders/1T2KgcalaRbLTiGgdPaIiN9mPwSY_6vdn
