# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
# убрать при nginx
EXPOSE 443

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Development
WORKDIR /src
COPY ["ArchiveFqp/ArchiveFqp/ArchiveFqp.csproj", "ArchiveFqp/ArchiveFqp/"]
COPY ["ArchiveFqp/ArchiveFqp.Client/ArchiveFqp.Client.csproj", "ArchiveFqp/ArchiveFqp.Client/"]
RUN dotnet restore "./ArchiveFqp/ArchiveFqp/ArchiveFqp.csproj"
COPY . .
WORKDIR "/src/ArchiveFqp/ArchiveFqp"
RUN dotnet build "./ArchiveFqp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Development
RUN dotnet publish "./ArchiveFqp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
# Примечание: шаг `dotnet dev-certs https --trust` здесь удалён. Он выполнялся в стадии
# сборки, а в финальный образ копируется только /app/publish, поэтому сертификат
# всё равно не попадал в контейнер, а сама команда падала с exit code 4.
# HTTPS настраивается извне: см. docker-compose.override.yml
# (ASPNETCORE_Kestrel__Certificates__Default__Path + монтирование ./certs).
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArchiveFqp.dll"]