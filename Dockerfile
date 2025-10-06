# ===== build =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ConferenceWebApp/ConferenceWebApp.csproj                          ConferenceWebApp/
COPY ConferenceWebApp.Application/ConferenceWebApp.Application.csproj  ConferenceWebApp.Application/
COPY ConferenceWebApp.Persistence/ConferenceWebApp.Persistence.csproj  ConferenceWebApp.Persistence/
COPY ConferenceWebApp.Infrastructure/ConferenceWebApp.Infrastructure.csproj  ConferenceWebApp.Infrastructure/
COPY ConferenceWebApp.Domain/ConferenceWebApp.Domain.csproj            ConferenceWebApp.Domain/

RUN dotnet restore ConferenceWebApp/ConferenceWebApp.csproj

COPY . .
RUN dotnet publish ConferenceWebApp/ConferenceWebApp.csproj -c Release -o /out /p:UseAppHost=false

# ===== runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

ARG APP_UID=10001
ARG APP_GID=10001

# создаём группу/пользователя только если их ещё нет
RUN set -eux; \
    if ! getent group app >/dev/null 2>&1; then \
        groupadd -g "${APP_GID}" app; \
    fi; \
    if ! id -u app >/dev/null 2>&1; then \
        useradd -r -u "${APP_UID}" -g app -s /usr/sbin/nologin app; \
    fi

WORKDIR /app
RUN mkdir -p /app/Logs /app/wwwroot

# владелец файлов — сразу app:app, поэтому отдельный chown не нужен
COPY --from=build --chown=app:app /out .

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0

USER app:app
EXPOSE 8080
ENTRYPOINT ["dotnet","ConferenceWebApp.dll"]
