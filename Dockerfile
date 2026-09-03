# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first (better layer caching)
COPY StudentResult/StudentResult.csproj StudentResult/
RUN dotnet restore StudentResult/StudentResult.csproj

# Build & publish
COPY . .
RUN dotnet publish StudentResult/StudentResult.csproj -c Release -o /app/publish

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# The SQLite file lives alongside the app (recreated & re-seeded on boot).
ENV DB_PATH=/app/studentresult.db
ENV ASPNETCORE_ENVIRONMENT=Production

# Render (and most container hosts) inject the port via $PORT; the app reads it.
EXPOSE 8080
ENTRYPOINT ["dotnet", "StudentResult.dll"]
