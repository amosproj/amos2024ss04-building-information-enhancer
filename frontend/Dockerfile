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

RUN npm run build

### Deploy the server ### 

# Use Apache HTTP server
FROM httpd:2.4

COPY --from=builder /usr/src/app/dist /usr/local/apache2/htdocs/

## Environment variables
ENV NODE_ENV=production
ENV PORT=80

# Expose the port
EXPOSE "${PORT}"

# Start web server
CMD ["httpd-foreground"]