# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

MyWeather ("get weather data by locations") is early-stage. The backend is a .NET 10 solution scaffold; there is no frontend yet and no tests.

## Layout

- Backend solution: `be/MyWeather.Api/MyWeather.slnx` (new `.slnx` format, so use .NET SDK 10+). Run all `dotnet` commands from `be/MyWeather.Api`.
- Projects: `MyWeather.WebApi` (startup project, ASP.NET Core minimal API), plus `MyWeather.Application`, `MyWeather.Domain`, `MyWeather.DAL`, `MyWeather.Infrastructure`.
- The class libraries are still empty `Class1.cs` stubs with **no `ProjectReference`s between projects**. The names suggest a Clean Architecture layering, but the dependency direction is not yet defined, so confirm it before wiring references.
- `MyWeather.WebApi/Program.cs` is still the template `/weatherforecast` endpoint with random data. No real weather data source has been chosen.

## Commands

Run from `be/MyWeather.Api`:

```
dotnet build MyWeather.slnx
dotnet run --project MyWeather.WebApi --launch-profile https   # https://localhost:7057, http://localhost:5020
dotnet watch run --project MyWeather.WebApi --launch-profile https
dotnet test MyWeather.slnx                                      # no test projects yet
dotnet test --filter "FullyQualifiedName~SomeTest"              # single test, once tests exist
```

`.vscode/` has matching tasks (`build`, `build-webapi`, `watch`, `test`) and an F5 launch config for `MyWeather.WebApi`. The launch config hardcodes `bin/Debug/net10.0`, so update it if the target framework changes. OpenAPI is mapped only in the Development environment.

## Branches

- Do all work on a feature branch created from `develop`.
- Feature branches are merged/pushed into `develop` only (PR targeting `develop`).
- Never push directly to `main`, and never target `main` with feature work.
