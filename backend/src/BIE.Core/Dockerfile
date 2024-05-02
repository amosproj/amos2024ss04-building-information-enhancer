#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BIE.Core.API/BIE.Core.API.csproj", "BIE.Core.API/"]
COPY ["BIE.Core.DataObjects/BIE.Core.DataObjects.csproj", "BIE.Core.DataObjects/"]
COPY ["BIE.Core.Services/BIE.Core.Services.csproj", "BIE.Core.Services/"]
COPY ["BIE.Core.BaseRepository/BIE.Core.BaseRepository.csproj", "BIE.Core.BaseRepository/"]
COPY ["BIE.Core.DBRepository/BIE.Core.DBRepository.csproj", "BIE.Core.DBRepository/"]
COPY ["BIE.Data/BIE.Data.csproj", "BIE.Data/"]
RUN dotnet restore "BIE.Core.API/BIE.Core.API.csproj"
COPY . .
WORKDIR "/src/BIE.Core.API"
RUN dotnet build "BIE.Core.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BIE.Core.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BIE.Core.API.dll"]
