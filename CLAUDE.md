# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

MyWeather ("get weather data by locations") is early-stage. The backend is a .NET 10 solution in `be/`; `fe/` is a Vite + React + TypeScript frontend built from the "Weather Favorites" Claude Design project.

## Layout

- Backend solution: `be/src/MyWeather.Api/MyWeather.slnx` (new `.slnx` format, so use .NET SDK 10+). Run all `dotnet` commands from `be/src/MyWeather.Api`.
- Projects: `MyWeather.WebApi` (startup project, ASP.NET Core minimal API), plus `MyWeather.Application`, `MyWeather.Domain`, `MyWeather.DAL`, `MyWeather.Infrastructure`.
- The class libraries are still empty `Class1.cs` stubs with **no `ProjectReference`s between projects**. The names suggest a Clean Architecture layering, but the dependency direction is not yet defined, so confirm it before wiring references.
- `MyWeather.WebApi/Program.cs` is still the template `/weatherforecast` endpoint with random data. No real weather data source has been chosen.

## Commands

Run from `be/src/MyWeather.Api`:

```
dotnet build MyWeather.slnx
dotnet run --project MyWeather.WebApi --launch-profile https   # https://localhost:7057, http://localhost:5020
dotnet watch run --project MyWeather.WebApi --launch-profile https
dotnet test MyWeather.slnx                                      # no test projects yet
dotnet test --filter "FullyQualifiedName~SomeTest"              # single test, once tests exist
dotnet ef migrations add <Name> --project MyWeather.DAL --startup-project MyWeather.WebApi   # after any model change
```

`.vscode/` has matching tasks (`build`, `build-webapi`, `watch`, `test`) and an F5 launch config for `MyWeather.WebApi`. The launch config hardcodes `bin/Debug/net10.0`, so update it if the target framework changes. OpenAPI is mapped only in the Development environment. In Development the app applies pending EF migrations on startup (`Database.Migrate()`); other environments run `dotnet ef database update`.

## Frontend

Run from `fe/` (Node comes from asdf; if a shell can't find `node`, put `~/.asdf/shims` on PATH, but not ahead of the system `dotnet`):

```
npm install
npm run dev     # http://localhost:5173, proxies /api/* to the API on https://localhost:7057 (https profile)
npm test        # Vitest + Testing Library
npm run build   # tsc + vite build
npm run lint    # oxlint
```

- Styling comes only from `src/styles/industry.css` (the design system's tokens and classes, copied verbatim) plus `src/styles/app.css` for layout. Use the `var(--*)` tokens rather than new hex values or fonts.
- Favorites are kept in browser `localStorage` because the API has no favorites endpoints yet. Hourly, 7-day, high/low, feels-like, air quality and sunrise/sunset are generated placeholders (`src/lib/sample.ts`) shown with a "Sample" tag, since the API returns current conditions only.



- Do all work on a feature branch created from `develop`.
- Feature branches are merged/pushed into `develop` only (PR targeting `develop`).
- Never push directly to `main`, and never target `main` with feature work.
