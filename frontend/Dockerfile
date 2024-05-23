# Load the node image from Docker Hub
FROM node:20-slim as builder

# Create app work directory
WORKDIR /usr/src/app

### Install dependencies ###

# Copy package.json and package-lock.json to the working directory
COPY package.json ./
COPY package-lock.json ./

# Install dependencies
RUN npm ci

# Upload the files
ADD . .

# Set environment variables for build
ENV VITE_BACKEND_HOST=localhost
ENV VITE_BACKEND_PORT=8081

RUN npm run build

### Deploy the server ###

# Use Apache HTTP server
FROM httpd:2.4

COPY --from=builder /usr/src/app/dist /usr/local/apache2/htdocs/

# Expose the port
EXPOSE "${PORT}"

# Start web server
CMD ["httpd-foreground"]