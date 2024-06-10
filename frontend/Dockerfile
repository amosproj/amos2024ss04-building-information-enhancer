# Load the node image from Docker Hub
FROM node:20-slim as builder

# Create app work directory
WORKDIR /usr/src/app

# Set environment variables 
ARG STAGE
ENV VITE_STAGE=${STAGE}

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

# Add the entrypoint script
COPY entrypoint.sh /usr/local/bin/
RUN chmod +x /usr/local/bin/entrypoint.sh

# Expose the port
EXPOSE 80

# Start web server using the entrypoint script
ENTRYPOINT ["entrypoint.sh"]
