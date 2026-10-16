# MyWeather frontend: the Vite build served by nginx, which also proxies /api to the API container.

# ---- build ----
FROM node:26-alpine AS build
WORKDIR /app

# Install dependencies first so this layer is cached until package-lock.json changes.
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ---- runtime ----
FROM nginx:1.28-alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
