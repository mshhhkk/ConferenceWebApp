
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src


COPY ../ ./


WORKDIR /src/ConferenceWebApp
RUN dotnet restore
RUN dotnet publish -c Release -o /out


FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /out ./


ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production


RUN mkdir -p /app/logs /app/wwwroot

EXPOSE 8080
ENTRYPOINT ["dotnet","ConferenceWebApp.dll"]
