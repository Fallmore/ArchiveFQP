# Информационная система учета выпускных работ

[![.NET](https://img.shields.io/badge/.NET-10.0-blue?logo=dotnet)](https://dotnet.microsoft.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly%20%7C%20Server-512BD4?logo=blazor)](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![Ollama](https://img.shields.io/badge/Ollama-llama3.2-000000?logo=ollama)](https://ollama.com/)

**ИС учета выпускных работ** — это веб-приложение для централизованного учета, хранения, поиска и выдачи выпускных квалификационных работ (ВКР) в вузах. Разработано в рамках выпускной квалификационной работы для Астраханского государственного технического университета (АГТУ).

Проект решает проблему неэффективного ручного документооборота, предоставляя единую базу данных с **интеллектуальным поиском** на основе локальной нейросети (Ollama + llama3.2) и **векторной базой** для семантического анализа.

---

## Основные возможности

- **Централизованный реестр ВКР**: Хранение всех работ с метаданными (студент, руководитель, год, институт, кафедра, направление, профиль и т.д.).
- **Многокритериальный поиск**: Быстрый поиск по любым полям, включая динамические атрибуты.
- **Автоматическое извлечение данных**: При добавлении работы локальная нейросеть (Ollama + all-minilm) автоматически считывает информацию с титульных листов и заполняет поля.
- **Динамические атрибуты**: Гибкая система EAV (Entity-Attribute-Value), позволяющая добавлять новые поля для работ в зависимости от института, кафедры или специальности.
- **Просмотр без скачивания**: Безопасный просмотр PDF-файлов с водяными знаками и запретом на копирование/печать.
- **Заявки на просмотр**: Студенты и преподаватели могут подавать заявки на выдачу работ, а сотрудники архива — управлять ими.
- **Ролевая модель**: Разграничение прав для 7 ролей (админ, студент, преподаватель, сотрудник архива, завкафедрой и др.).
- **Интеграция с Moodle**: Авторизация через образовательный портал АГТУ (OAuth2).
- **Генерация отчетов**: Автоматическое формирование отчетов в формате Word по результатам поиска.

---

## Технологический стек

| Компонент | Технология |
| :--- | :--- |
| **Frontend / UI** | [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) (.NET 10) |
| **Backend API** | ASP.NET Core (C#) |
| **База данных** | [PostgreSQL 16](https://www.postgresql.org/) + Entity Framework Core |
| **Локальная LLM** | [Ollama](https://ollama.com/) (модель `llama3.2`) |
| **Векторная БД** | Интегрирована через `all-minilm` (семантический поиск) |
| **Сервис извлечения данных** | Python (FastAPI) + Ollama |
| **Логирование** | Журнал действий в БД |

---

## Примеры интерфейсов

| Галерея работ | Просмотр ПЗ | Добавление работ | Настройки кафедр |
| :---: | :---: | :---: | :---: |
| <img width="1586" height="778" alt="image" src="https://github.com/user-attachments/assets/82f00362-5d32-4b61-b417-ca0f4c4d3cd0" /> |  <img width="1584" height="776" alt="image" src="https://github.com/user-attachments/assets/2b736dd0-c278-492f-95e3-2318cadf1790" /> | <img width="850" height="669" alt="image" src="https://github.com/user-attachments/assets/54ea779d-c8d3-4740-a70c-51e5a7ee6591" /> | <img width="1167" height="716" alt="image" src="https://github.com/user-attachments/assets/9bc22e51-4ec8-47a8-8db6-62e81fec9d93" /> |

---

## Быстрый старт

### Установка и запуск

Вы можете запустить приложение одним из трех способов, в зависимости от ваших целей и операционной системы.

#### Предварительные требования (для всех способов)

Перед запуском убедитесь, что у вас установлены необходимые компоненты:

1.  **[.NET 10 SDK](https://dotnet.microsoft.com/download)** — обязателен для сборки и запуска проекта.
2.  **[PostgreSQL 16](https://www.postgresql.org/download/)** — база данных. Создайте базу данных (например, `archive_fqp`) и восстановите структуру из файла `backup.sql`, который находится в `Utilities.zip` в релизе.
3.  **(Опционально) [Ollama](https://ollama.com/download)** — для работы локальной нейросети. После установки скачайте модели:
    ```bash
    ollama pull llama3.2
    ollama pull all-minilm
    ```
    Этот шаг необходим, если вы планируете использовать автоматическое извлечение данных из PDF-файлов.

---

#### 1. Разработка (Windows / Linux)

Этот способ подходит для внесения изменений в код и отладки.

1.  **Клонируйте репозиторий**:
    ```bash
    git clone https://github.com/Fallmore/ArchiveFQP.git
    cd ArchiveFQP
    ```
2.  **Восстановите зависимости**:
    ```bash
    dotnet restore
    ```
    *(Все необходимые пакеты NuGet будут загружены автоматически).*

3.  **Настройте подключение к базе данных**:
    - Откройте файл `ArchiveFqp/appsettings.json`.
    - Измените строку подключения `DefaultConnection` в разделе `ConnectionStrings`, указав ваш сервер PostgreSQL, логин, пароль и имя базы данных (например, `archive_fqp`).

4.  **Примените миграции (опционально)**:
    - Если база данных еще не создана, выполните:
      ```bash
      dotnet ef database update --project ArchiveFqp
      ```
    - Или восстановите базу данных из файла одного из [релизов](https://github.com/Fallmore/ArchiveFQP/releases) `backup.sql`.

5.  **Запустите приложение**:
    - Откройте решение `ArchiveFQP.sln` в **Visual Studio 2026** и нажмите `F5` (или `Ctrl+F5`).
    - **ИЛИ** из командной строки:
      ```bash
      dotnet run --project ArchiveFqp
      ```
6.  Откройте в браузере адрес, указанный в консоли (обычно `https://localhost:5001`).

---

#### 2. Релиз (Windows x64)

Этот способ для быстрого запуска на Windows-сервере без установки Visual Studio.

1.  **Скачайте релиз**:
    - Перейдите на страницу [релизов](https://github.com/Fallmore/ArchiveFQP/releases) и скачайте архив **`win-x64 debug.zip`**.

2.  **Распакуйте архив**:
    - Распакуйте содержимое в папку на сервере (например, `C:\ArchiveFQP`).
    - Внутри вы найдете:
        - Папку `publish/` — с самим приложением.
        - Файл `Utilities.zip` — с `AiExtractor` и бэкапом `backup.sql`.

3.  **Настройте базу данных**:
    - Установите PostgreSQL 16.
    - Восстановите базу данных из `backup.sql` (распаковав предварительно `Utilities.zip`).

4.  **Настройте конфигурацию**:
    - В папке `publish/` откройте файл `appsettings.json`.
    - Пропишите вашу строку подключения к PostgreSQL в разделе `ConnectionStrings`.

5.  **Запустите приложение**:
    - В папке `publish/` найдите и запустите исполняемый файл `ArchiveFqp.exe`.

---

#### 3. Релиз (Linux x64)

Этот способ для установки и запуска на Linux-сервере (например, Ubuntu) в качестве фонового сервиса.

1.  **Скачайте релиз**:
    - Скачайте архив **`linux-x64 debug.zip`** со страницы [релизов](https://github.com/Fallmore/ArchiveFQP/releases).

2.  **Загрузите и распакуйте на сервер**:
    - Скопируйте архив на сервер (например, в `/home/userpc/ArchiveFQP/`).
    - Распакуйте его:
      ```bash
      unzip linux-x64\ debug.zip -d /home/userpc/ArchiveFQP/
      ```
    - Внутри будут папки `publish/` и `Utilities.zip`.

3.  **Настройте базу данных**:
    - Установите PostgreSQL и настройте его (см. системные требования).
    - Восстановите базу данных из `backup.sql` (из `Utilities.zip`).

4.  **Настройте конфигурацию**:
    - Отредактируйте файл `/home/userpc/ArchiveFQP/publish/appsettings.json`, указав правильные строки подключения к БД и адрес сервиса AiExtractor (если он используется).

5.  **Настройте сервис `systemd` (для автозапуска)**:
    - Создайте файл службы `/etc/systemd/system/archive-fqp.service` со следующим содержимым (пути укажите свои):
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
    - Запустите и включите автозапуск:
      ```bash
      sudo systemctl daemon-reload
      sudo systemctl enable archive-fqp.service
      sudo systemctl start archive-fqp.service
      ```

6.  **Проверьте статус**:
    - Статус службы можно проверить командой:
      ```bash
      sudo systemctl status archive-fqp.service
      ```

---

#### Запуск сервиса локальной нейросети (AiExtractor) — Опционально

Сервис для извлечения данных из PDF с помощью Ollama. Может работать как на Windows, так и на Linux.

1.  Установите **Python 3.12**.
2.  Распакуйте папку `AiExtractor` из `Utilities.zip`.
3.  Перейдите в папку с сервисом и установите зависимости:
    ```bash
    pip install -r requirements.txt
    ```
4.  Отредактируйте файл `config.py`, указав параметры подключения к PostgreSQL, Ollama и другие настройки.
5.  Запустите сервис:
    ```bash
    python api_server.py
    ```
    *(На Linux рекомендуется настроить отдельный systemd-сервис `ai-extractor.service` для автозапуска, как было описано выше).*

---

## Контакты

Почта: slava_samodurov@mail.ru

---

## Подробная документация
https://drive.google.com/drive/u/0/folders/1T2KgcalaRbLTiGgdPaIiN9mPwSY_6vdn
