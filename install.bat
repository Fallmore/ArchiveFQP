@echo off
rem Сборка и запуск всех сервисов, затем вывод логов.
rem Имена сервисов: postgres, archivefqp (см. docker-compose.yml).
rem Остановка: docker compose down

docker compose build
if errorlevel 1 goto :error

docker compose up -d
if errorlevel 1 goto :error

docker compose ps
echo.
echo Приложение: https://localhost:8443 (HTTP: http://localhost:8001)
echo.
docker compose logs -f
goto :eof

:error
echo Ошибка сборки или запуска. См. вывод выше.
exit /b 1