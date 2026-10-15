# MyWeather
get weather data by locations

## Run with Docker

Needs only Docker (Desktop) and an [OpenWeatherMap API key](https://openweathermap.org/api).

```sh
cp .env.example .env        # then set OPENWEATHER_API_KEY in .env
docker compose up --build   # add -d to run in the background
```

- App: http://localhost:8080 (nginx serves the frontend and forwards `/api/*` to the API)
- API: http://localhost:5020, with interactive docs at http://localhost:5020/docs

The SQLite database lives in the `myweather-data` Docker volume, so favorites survive `docker compose down`.
`docker compose down -v` deletes it; the next start recreates and seeds it.
