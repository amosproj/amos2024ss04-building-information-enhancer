# Use the official MongoDB image from Docker Hub
FROM mongo:latest

# Copy the initialization script
COPY ./init-db.js /docker-entrypoint-initdb.d/

# Expose the default MongoDB port
EXPOSE 27017
