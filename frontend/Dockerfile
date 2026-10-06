FROM node:22-alpine AS build
ARG VITE_API_URL
ARG VITE_CSP_SCRIPT_SRC
ARG VITE_CSP_STYLE_SRC
ARG VITE_CSP_CONNECT_SRC
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM nginx:alpine
RUN rm /etc/nginx/conf.d/default.conf
COPY nginx.conf /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist /usr/share/nginx/html
