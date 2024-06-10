FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
workdir /app

expose 445

USER root

RUN apt-get update && apt-get install -y dos2unix

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
workdir /app

COPY *.csproj .
RUN dotnet restore

copy . ./
RUN dotnet publish "BIE.DataPipeline.csproj" --output /app/ --configuration Release

from base as final
workdir /app
copy --from=build /app .

RUN dos2unix entrypoint.sh

ENTRYPOINT ["/bin/bash","entrypoint.sh"]
