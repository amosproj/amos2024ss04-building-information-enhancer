FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
workdir /app

expose 445

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
workdir /app

COPY *.csproj .
RUN dotnet restore

copy . ./
RUN dotnet publish --output /app/ --configuration Release

from base as final
workdir /app
copy --from=build /app .

entrypoint ["dotnet","BIE.DataPipeline.dll"]