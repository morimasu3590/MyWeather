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

# Architecure Overview

be/src/MyWeather.Api · MyWeather.slnx · net10.0

## MyWeather Backend Layers

How the five backend projects reference each other, placed on the Clean Architecture rings. Every reference points inward, toward Domain.

## Project references

Arrows are `<ProjectReference>` entries, labeled with what crosses the boundary. Teal text marks the interfaces (ports) an inner project defines and an outer project implements. DAL skips the Application ring because its repository interfaces live in Domain. Nothing references WebApi except its test project.

## What each project holds

| Project | Ring | References | NuGet packages | Holds |
| --- | --- | --- | --- | --- |
| `MyWeather.Domain` | Domain | none | none | Entities `City`, `Country`, `Weather`, `FavoriteCity`; repository interfaces; temperature conversion |
| `MyWeather.Application` | Application | Domain | MediatR, FluentValidation | Query and command handlers, request DTOs and validators, `IOpenWeatherMapClient` and its response DTOs |
| `MyWeather.DAL` | Outer · adapter | Domain | EF Core, EF Core Relational | `MyWeatherDbContext`, entity configurations, country and city seed data, migrations, the four repositories |
| `MyWeather.Infrastructure` | Outer · adapter | Application | Caching.Memory, Logging.Abstractions, Polly.Core | `OpenWeatherMapClient` (geo lookup cached for a day), API-key message handler |
| `MyWeather.WebApi` | Outer · delivery | Application, DAL, Infrastructure | EF Core Sqlite + Design, Http.Resilience, FluentValidation DI, OpenApi, Scalar, DotNetEnv | Endpoint groups `/countries`, `/weathers`, `/favorites`; DI wiring; startup migrations in Development |

Test projects follow the same shape: `Domain.Tests`, `Application.Tests`, `DAL.Tests`, `Infrastructure.Tests` and `WebApi.Tests` each reference only the project they test.

## Clean Architecture check

- Holds

  **Domain depends on nothing.** It has no project references and no NuGet packages.
- Holds

  **Every reference points inward.** No inner project references an outer one, and Domain and Application contain no EF Core or ASP.NET Core `using` directives.
- Holds

  **Outer projects implement inner interfaces.** DAL implements the repository interfaces from Domain; Infrastructure implements `IOpenWeatherMapClient` from Application.
- Holds

  **WebApi only wires and delegates.** Endpoints talk to Application through MediatR's `ISender`. WebApi references DAL and Infrastructure only to register them and run migrations, which is the composition root's job.
- Consider

  **Domain entities carry mapping attributes.** `[Key]` and `[ForeignKey]` on the four entities are persistence details. DAL already has entity configurations, so the mapping could move there and leave Domain persistence-free.
- Consider

  **The weather port speaks OpenWeatherMap's format.** `OpenWeatherMapCurrentResponseDto` and `OpenWeatherMapGeoResponseDto` live in Application, so handlers depend on one provider's JSON shape. A provider-neutral port, with Infrastructure doing the mapping, would let you swap providers without touching handlers.
- Consider

  **Ports live in two places.** Repository interfaces are in Domain; the weather client interface is in Application. Both are valid, but picking one home makes it clearer where the next port goes.