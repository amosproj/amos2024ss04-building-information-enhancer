# Use the official Microsoft .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the .csproj file and restore any dependencies
# This is done separately from the rest of the source to (potentially) 
# speed up later builds.
COPY ./api-gateway/APIGateway.csproj ./api-gateway/
RUN dotnet restore ./api-gateway/APIGateway.csproj

# Copy the remaining source code
COPY ./api-gateway ./api-gateway
# Copy the Metadata Library
COPY ./lib/BieMetadata/bin ./lib/BieMetadata/bin

# Build the application
RUN dotnet publish ./api-gateway/APIGateway.csproj -c Release -o out

# Use the official Microsoft .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the build output from the build stage
COPY --from=build /app/out .

# Expose the port the application will run on
EXPOSE 8080

# Start the application
ENTRYPOINT ["dotnet", "APIGateway.dll"]
