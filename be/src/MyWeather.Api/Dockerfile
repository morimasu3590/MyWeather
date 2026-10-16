# MyWeather API. Build context: be/src/MyWeather.Api (see docker-compose.yml at the repository root).

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first, from the project files only, so this layer is cached until a dependency changes.
# Test projects are not needed to run the API, so they are neither copied nor built.
COPY MyWeather.Domain/MyWeather.Domain.csproj MyWeather.Domain/
COPY MyWeather.Application/MyWeather.Application.csproj MyWeather.Application/
COPY MyWeather.DAL/MyWeather.DAL.csproj MyWeather.DAL/
COPY MyWeather.Infrastructure/MyWeather.Infrastructure.csproj MyWeather.Infrastructure/
COPY MyWeather.WebApi/MyWeather.WebApi.csproj MyWeather.WebApi/
RUN dotnet restore MyWeather.WebApi/MyWeather.WebApi.csproj

COPY MyWeather.Domain/ MyWeather.Domain/
COPY MyWeather.Application/ MyWeather.Application/
COPY MyWeather.DAL/ MyWeather.DAL/
COPY MyWeather.Infrastructure/ MyWeather.Infrastructure/
COPY MyWeather.WebApi/ MyWeather.WebApi/
RUN dotnet publish MyWeather.WebApi/MyWeather.WebApi.csproj -c Release -o /app --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080

# The SQLite database lives in /data (a volume in docker-compose.yml). The directory is owned by the
# non-root app user, and a new named volume inherits that ownership.
RUN mkdir /data && chown "$APP_UID" /data

COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "MyWeather.WebApi.dll"]
