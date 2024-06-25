# Prepare the basline image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

EXPOSE 445

USER root
RUN apt-get update && apt-get install -y dos2unix

# Build the dotnet project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

COPY *.csproj .
RUN dotnet restore

COPY . ./

RUN dotnet publish "BIE.DataPipeline.csproj" --output /app/ --configuration Release

# Run the project
FROM base AS final
WORKDIR /app
COPY --from=build /app .

RUN dos2unix entrypoint.sh

ENTRYPOINT ["/bin/bash","entrypoint.sh"]
