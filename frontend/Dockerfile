# Stage 1: Build the frontend
FROM node:slim AS build
WORKDIR /app
COPY . .

# Inject build-time environment variables
ARG VITE_API_BASE_URL
ARG VITE_SIGNALR_HUB_URL
ARG VITE_FRONTEND_QR_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_SIGNALR_HUB_URL=$VITE_SIGNALR_HUB_URL
ENV VITE_FRONTEND_QR_URL=$VITE_FRONTEND_QR_URL


RUN npm install
RUN npm run build:docker

# Stage 2: Serve with Nginx
FROM nginx:stable-alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist .

# Optional: overwrite default nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf
