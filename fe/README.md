# MyWeather frontend (Weatherboard)

A React + TypeScript app, built with Vite, for tracking the weather in up to 3 favorite cities. It implements the "Weather Favorites" design: a dashboard of city columns, an Add city dialog, and a detail view with a city rail. It reads data from the MyWeather API in `../be`.

## Prerequisites

- **Node.js 22 or newer and npm.** This repo was set up with Node 26 installed through [asdf](https://asdf-vm.com). If your shell can't find `node`, add `~/.asdf/shims` to `PATH`.
- **.NET SDK 10**, for the API.

> **asdf and `dotnet`:** if `~/.asdf/shims` comes *before* the system `dotnet` on `PATH`, `dotnet` fails with "No version is set for command dotnet". Keep the asdf shims after the system dotnet, or start the API in a shell without them:
>
> ```sh
> PATH="$(echo "$PATH" | tr ':' '\n' | grep -v '\.asdf' | paste -sd: -)" dotnet --version
> ```

## Run it locally

You need two terminals.

**1. Start the API** (from the repo root):

```sh
cd be/src/MyWeather.Api
dotnet run --project MyWeather.WebApi --launch-profile https   # https://localhost:7057 (and http://localhost:5020)
```

In Development the API applies its database migrations on startup. The SQLite file `myweather.db` is created in the repo root, seeded with 20 countries and their capitals.

**2. Start the frontend:**

```sh
cd fe
npm install
npm run dev     # http://localhost:5173
```

Open http://localhost:5173. The dev server forwards every `/api/*` request to the API (`/api/countries` becomes `https://localhost:7057/countries`), so no CORS setup is needed. The proxy accepts the self-signed ASP.NET Core development certificate.

> **Why HTTPS:** the API redirects plain HTTP to `https://localhost:7057`. If the proxy pointed at `http://localhost:5020`, the browser would follow that redirect to a different origin, and CORS would block it ("No 'Access-Control-Allow-Origin' header"). If you run the API with the `http` profile instead (port 5020 only, no redirect), start the frontend with `VITE_API_PROXY_TARGET=http://localhost:5020 npm run dev`.

### Pointing at another API

| Variable | Used by | Default |
| --- | --- | --- |
| `VITE_API_PROXY_TARGET` | the dev server proxy (`vite.config.ts`) | `https://localhost:7057` |
| `VITE_API_BASE_URL` | the app's fetch calls (`src/api/client.ts`) | `/api` |

For example: `VITE_API_PROXY_TARGET=http://localhost:5020 npm run dev`. You can also put these in a `fe/.env.local` file, which git ignores.

## Scripts

| Command | What it does |
| --- | --- |
| `npm run dev` | Dev server with hot reload |
| `npm test` | Runs the tests once (Vitest + Testing Library, jsdom) |
| `npm run test:watch` | Runs the tests in watch mode |
| `npm run build` | Type-checks (`tsc -b`), then builds to `dist/` |
| `npm run preview` | Serves the built `dist/` locally |
| `npm run lint` | Lints with oxlint |

## What to expect

- **Favorites** are saved in your browser's `localStorage`, under the key `myweather.favorites`. The API has no favorites endpoints yet, so they don't follow you to another browser.
- **Weather** comes from `GET /weathers/{cityName}`. The database has no weather rows yet, so every city shows **"No weather data yet"** until weather data is added to the backend.
- **Sample data:** the API returns current conditions only. The hourly bars, 7-day forecast, high/low, feels-like, air quality and sunrise/sunset are generated placeholders, and each one carries a **Sample** tag.

## Project layout

```
fe/
├── index.html
├── vite.config.ts          # dev proxy and Vitest config
└── src/
    ├── main.tsx            # entry point, loads the stylesheets
    ├── App.tsx             # overview / detail / dialog state
    ├── api/                # typed fetch client and API response types
    ├── components/         # AppHeader, CityColumn, CityDetail, AddCityDialog, SearchSelect, …
    ├── lib/                # unit formatting, view model, sample forecast generator
    ├── state/              # useFavorites (localStorage), useWeather (API)
    ├── styles/
    │   ├── industry.css    # design system tokens + classes, copied verbatim from the design
    │   └── app.css         # layout for this app, built only from industry.css tokens
    └── test/               # test setup and fixtures
```

Use the `var(--*)` tokens from `industry.css` for colors, fonts and shadows rather than adding new hex values or font families.
