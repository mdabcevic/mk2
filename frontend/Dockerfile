# Stage 1: Build the frontend
FROM node:slim AS build
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build:docker

# Stage 2: Serve with Nginx
FROM nginx:stable-alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist .

# Optional: overwrite default nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf
