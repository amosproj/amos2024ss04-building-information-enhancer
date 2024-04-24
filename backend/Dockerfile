# Load the node image from Docker Hub
FROM node:16 as builder

# Create app directory
WORKDIR /usr/src/backend

### Install dependencies ### TO-DO

# Copy package.json and package-lock.json to the working directory
# COPY frontend/package*.json ./
# RUN npm ci

ADD ./backend .

### Build ### TO-DO

# RUN npm run build

# Expose the port
EXPOSE "${PORT}"

### Start the server ### TO-DO