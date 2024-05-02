# Load the node image from Docker Hub
FROM node:20-slim as builder

# Create app work directory
WORKDIR /app

### Install dependencies ###

# Copy package.json and package-lock.json to the working directory
COPY frontend/package*.json ./

# Install dependencies
RUN npm ci

# Upload the files
ADD ./frontend .

RUN npm run build

COPY . .

### Deploy the server ### 

# Use Nginx as lightweight production web server
FROM nginx:stable-alpine

COPY --from=builder /app/dist /usr/share/nginx/html

## Environment variables
ENV NODE_ENV production
ENV PORT=80

# Expose the port
EXPOSE "${PORT}"

# Start NGINX server in the background
CMD ["nginx", "-g", "daemon off;"]