# ── Build Stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY AidenApi/AidenApi.csproj AidenApi/
RUN dotnet restore AidenApi/AidenApi.csproj

COPY AidenApi/ AidenApi/
WORKDIR /src/AidenApi
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime Stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Non-root user for security
RUN addgroup -S aiden && adduser -S aiden -G aiden
USER aiden

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000

ENTRYPOINT ["dotnet", "AidenApi.dll"]
