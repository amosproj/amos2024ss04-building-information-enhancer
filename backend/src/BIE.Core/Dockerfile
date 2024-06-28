#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

### COPY PROJECTS ###
ENV C_DIR=src/BIE.Core
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source
COPY ["src/BIE.Core/BIE.Core.API/BIE.Core.API.csproj", "src/BIE.Core/BIE.Core.API/"]
COPY ["src/BIE.Core/BIE.Core.DataObjects/BIE.Core.DataObjects.csproj", "src/BIE.Core/BIE.Core.DataObjects/"]
COPY ["src/BIE.Core/BIE.Core.Services/BIE.Core.Services.csproj", "src/BIE.Core/BIE.Core.Services/"]
COPY ["src/BIE.Core/BIE.Core.BaseRepository/BIE.Core.BaseRepository.csproj", "src/BIE.Core/BIE.Core.BaseRepository/"]
COPY ["src/BIE.Core/BIE.Core.DBRepository/BIE.Core.DBRepository.csproj", "src/BIE.Core/BIE.Core.DBRepository/"]
COPY ["src/BIE.Core/BIE.Data/BIE.Data.csproj", "src/BIE.Core/BIE.Data/"]
RUN dotnet restore "src/BIE.Core/BIE.Core.API/BIE.Core.API.csproj"
COPY "./src/BIE.Core" "./src/BIE.Core"

### BUILD ###
# Copy and build metadata library
COPY ./lib/BieMetadata ./lib/BieMetadata
RUN dotnet build "./lib/BieMetadata/BieMetadata.csproj" -c Release

WORKDIR "/source/src/BIE.Core/BIE.Core.API"
RUN dotnet build "BIE.Core.API.csproj" -c Release -o /app/build

### PUBLISH ###
FROM build AS publish
RUN dotnet publish "BIE.Core.API.csproj" -c Release -o /app/publish

### DEPLOY ###
FROM base AS final

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BIE.Core.API.dll"]
