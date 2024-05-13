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

COPY . .

### Deploy the server ### 

# Use Nginx as lightweight production web server
FROM nginx:stable-alpine

COPY --from=builder /usr/src/app/dist /usr/share/nginx/html

## Environment variables
ENV NODE_ENV production
ENV PORT=80

# Expose the port
EXPOSE "${PORT}"

# Start NGINX server in the background
CMD ["nginx", "-g", "daemon off;"]