# Dockerfile

# Load the node image from Docker Hub
FROM node:20-slim as builder

# Create app work directory
WORKDIR /usr/src/app

# Set environment variables 
ARG STAGE
ENV VITE_STAGE=${STAGE}

# Install dependencies
COPY package.json ./
COPY package-lock.json ./
RUN npm ci

# Upload the files
ADD . .
RUN npm run build

# Deploy the Apache HTTP
FROM httpd:2.4-alpine

# Install dos2unix to convert line endings
RUN apk add --no-cache dos2unix

COPY --from=builder /usr/src/app/dist /usr/local/apache2/htdocs/
COPY replace_api_url.sh /usr/local/apache2/htdocs/
COPY httpd.conf /usr/local/apache2/conf/httpd.conf

# Convert line endings and ensure the script is executable
RUN dos2unix /usr/local/apache2/htdocs/replace_api_url.sh && chmod +x /usr/local/apache2/htdocs/replace_api_url.sh

# Set default values for environment variables
ENV API_GATEWAY_HOST="localhost"
ENV API_GATEWAY_PORT="8081"

# Expose the port
EXPOSE "${PORT}"

# Start web server after running the script
CMD sh /usr/local/apache2/htdocs/replace_api_url.sh "${API_GATEWAY_HOST}" "${API_GATEWAY_PORT}"
