# Stage 1: Build the frontend
FROM node:slim AS build
WORKDIR /app
COPY . .

# Accept args from the GitHub Actions workflow
ARG VITE_API_BASE_URL
ARG VITE_SIGNALR_HUB_URL
ARG VITE_FRONTEND_QR_URL

# Write args into a Vite-compatible .env.production file
RUN echo "VITE_API_BASE_URL=$VITE_API_BASE_URL" > .env.production && \
    echo "VITE_SIGNALR_HUB_URL=$VITE_SIGNALR_HUB_URL" >> .env.production && \
    echo "VITE_FRONTEND_QR_URL=$VITE_FRONTEND_QR_URL" >> .env.production

RUN npm install
RUN npm run build:docker

# Stage 2: Serve with Nginx
FROM nginx:stable-alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/dist .

# Optional: overwrite default nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf
