<div align="center">
  <div align="center" style="background-color:#0d1117; padding-bottom: 20px; border-radius:10px">
    <picture>
        <source media="(prefers-color-scheme: dark)" srcset="https://github.com/amosproj/amos2024ss04-building-information-enhancer/blob/main/Deliverables/sprint-01/team-logo.png?raw=true">
        <source media="(prefers-color-scheme: light)" srcset="https://github.com/amosproj/amos2024ss04-building-information-enhancer/blob/main/Deliverables/sprint-01/team-logo-black.png?raw=true">
        <img src="https://github.com/amosproj/amos2024ss04-building-information-enhancer/blob/main/Deliverables/sprint-01/team-logo.png?raw=true" width="400" alt="Code.ing Group Logo">
    </picture>
    </br>
  </div>
  <h1 style="padding:15px;border-bottom: 0;">Building Information Enhancer (AMOS SS 2024)</h1>
</div>

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
![GitHub language count](https://img.shields.io/github/languages/count/amosproj/amos2024ss04-building-information-enhancer)
![GitHub last commit](https://img.shields.io/github/last-commit/amosproj/amos2024ss04-building-information-enhancer)
![GitHub issues](https://img.shields.io/github/issues/amosproj/amos2024ss04-building-information-enhancer)

## 📢 About the Project

Welcome to the official repository for the `CODE.ING` group, developing an open-source `Building Information Enhancer` software for the `BUILD.ING` partner as a part of the `AMOS 2024` project. 

The `BCI Building Information Enhancer` is a platform for individual building owners or industry professionals, allowing them to access information about specific addresses, locations, or regions. This information can be used for a variety of applications, from sustainability certifications for buildings, over calculating the solar power potential, up to aiding in district planning. In general, the BCI building information enhancer offers significant benefits for various stakeholders in the property market.

For our project mission, the team agreed to create an MVP for the BCI Building Information Enhancer, the core functionality will be displaying data from a fixed number of sources, including satellite images, charging stations, and data needed for sustainability certification. Our goal is to build a practical and scalable tool that can grow with our users' needs. Finally, to read about the architecture of our service visit our [GitHub Wiki Pages](https://github.com/amosproj/amos2024ss04-building-information-enhancer/wiki).

## 🚀 Setup & Deployment

`BCI Building Information Enhancer` project is integrated with a full [CI-CD pipeline](https://github.com/amosproj/amos2024ss04-building-information-enhancer/wiki/CI%E2%80%90CD-Pipeline) and thus deployed  automatically for public use as a [BCI website](http://prod.amos.b-ci.de/) using servers provided by the industry partner. However, production servers might be turned off in the future, so it is also possible to deploy a production-ready system on your local machine using the provided Docker compose file.

## 📦 Deployment using Docker

To deploy the latest release of the project on your local machine using Docker Engine, follow the instructions below:

1. Clone this repository (https://github.com/amosproj/amos2024ss04-building-information-enhancer).
2. Before deploying the project be sure you have installed the [Node.js](https://nodejs.org/en) ( >= 20.12.2) and a running [Docker Engine](https://docs.docker.com/engine/install/).
3. Run the `npm run deploy` command in the root folder of the repository. \*
4. Connect to the frontend at port 80 (`localhost:80`).

\* This command pulls the newest public release of the images, if you want to build your local files, use `npm run deploy:build` instead.

## 💻 Developement Deployment

In order to deploy and/or develop one or more services, follow the instructions below:

### Setup

1. Clone this repository (https://github.com/amosproj/amos2024ss04-building-information-enhancer).
2. Before deploying the project make sure you have installed the [Node.js](https://nodejs.org/en) ( >= 20.12.2), [Dotnet SDK](https://dotnet.microsoft.com/en-us/download) ( >= 8.0.3) and a running [Docker Engine](https://docs.docker.com/engine/install/).
3. Run the `npm run setup:<os>` command to download all necessary packages and build the services, replacing `<os>` with `windows`, `linux` or `macos` based on your operating system.

### Developement

By following the micro-service architecture, each of the services can be deployed and developed separately. However, some of the services depend on each other. Follow instructions below to develop a specific service. Finally, the recommender way to deploy individual services is the docker images.

- **Frontend**: To develop the frontend service, one can run the `npm run deploy:frontend:dev` to deploy the VITE development server (with hot reloading). The code is located in the `frontend` folder. At a minimum, the frontend depends on the API Gateway and the MongoDB metadata database. If you want to query the datasets, the API Composer and the Geospatial DB needs to be deployed too. To deploy the service as a container use `docker compose up --build -d frontend`
- **API Gateway**: To develop the C# API Gateway, first make your changes in the `backend/API-Gateway` folder and run inside of it the `dotnet build` command to build the executables. Finally, to run the Gateway run type the `dotnet run` command. The API Gateway can run on its own, however, to use one of its API endpoints, one needs to deploy the corresponding service (for more information about the endpoints, read our [System Architecture](https://github.com/amosproj/amos2024ss04-building-information-enhancer/wiki/System-Architecture) wiki page). To deploy the service as a container use `docker compose up --build -d api-gateway`.
- **API Composer**: Similarly to the API Gateway, go to the corresponding folder in the `backend` directory, make the changes, and run `dotnet build` and `dotnet run`. To deploy the service as a container use `docker compose up --build -d api-composer`.
- **Data Pipeline**: Similarly to the API Gateway, go to the corresponding folder in the `backend` directory, make the changes, and run `dotnet build` and `dotnet run`. To deploy the service as a container use `docker compose up --build -d datapipeline`.
- **Metadata Database**: To deploy the MongoDB database one needs to use the Docker `docker compose up --build -d metadata-database` command in the root folder. In order to change the metadata, edit the `init-db.js` file inside `backend/metadata-database` folder.
- **Geospatial Databases**: To deploy the Geospatial databases one needs to use the Docker `docker compose up --build -d sql-database` command in the root folder.

## 👥 The Team

[![Contributors](https://contrib.rocks/image?repo=amosproj/amos2024ss04-building-information-enhancer)](https://github.com/amosproj/amos2024ss04-building-information-enhancer/graphs/contributors)
