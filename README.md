<div align="center">
  <div align="center" style="background-color:#0d1117; padding-bottom: 20px; border-radius:10px">
    <img src="https://github.com/amosproj/amos2024ss04-building-information-enhancer/blob/main/Deliverables/sprint-01/team-logo.png?raw=true" width="400" alt="Code.ing Group Logo">
    </br>
  </div>
  <h1 style="padding:15px;border-bottom: 0;">Building Information Enhancer (AMOS SS 2024)</h1>
</div>

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)
![GitHub language count](https://img.shields.io/github/languages/count/amosproj/amos2024ss04-building-information-enhancer)
![GitHub last commit](https://img.shields.io/github/last-commit/amosproj/amos2024ss04-building-information-enhancer)
![GitHub issues](https://img.shields.io/github/issues/amosproj/amos2024ss04-building-information-enhancer)

## 📢 About the Project

Welcome to the official repository for the `CODE.ING` group, developing an open-source `Building Information Enhancer` software for the `BUILD.ING` partner and the `AMOS 2024` project. Together, we create a Building information system for potential energy savings. To read about the architecture of our service visit our GitHub [wiki](https://github.com/amosproj/amos2024ss04-building-information-enhancer/wiki).

## 🚀 Setup & Deployment

This project is integrated with a full CI/CD pipeline, thus deployed automatically on the web. However, it is possible to deploy a production-ready system on your local machine using Docker. For further information on the `CI/CD` pipeline visit our GitHub [wiki](https://github.com/amosproj/amos2024ss04-building-information-enhancer/wiki/CI%E2%80%90CD-Pipeline) documentation.

## 📦 Deployment using Docker

To deploy the latest release of the project on your local machine using Docker Engine, follow the instructions below:

1. Clone this repository (https://github.com/amosproj/amos2024ss04-building-information-enhancer).
2. Before deploying the project be sure you have installed the [Node.js](https://nodejs.org/en) ( >= 20.12.2) and a running [Docker Engine](https://docs.docker.com/engine/install/).
3. Run the `npm run deploy` command in the root folder of the repository. \*
4. Connect to the frontend at port 80 (`localhost:80`).

\* This command pulls the newest release of the images, if you want to build your local files, use `npm run deploy:build` instead.

## 💻 Developement Deployment

In order to deploy and/or develop one or more services, follow the instructions below:

### Setup

1. Clone this repository (https://github.com/amosproj/amos2024ss04-building-information-enhancer).
2. Before deploying the project make sure you have installed the [Node.js](https://nodejs.org/en) ( >= 20.12.2) and [Dotnet SDK](https://dotnet.microsoft.com/en-us/download) ( >= 8.0.3).
3. Run the `npm run setup:<os>` command to download all necessary packages and build the services, replacing `<os>` with `windows`, `linux` or `macos` based on your operating system.

### Developement

1. If you want to develop the frontend service, run the `npm run deploy:backend:api` to deploy the backend, and in a new terminal run the `npm run deploy:frontend:dev` command to deploy the frontend development server.
2. If you want to develop one of the backend services, instead you need to make the changes and re-build the backend executables afterward. To do so, go to the `backend` folder and open the corresponding `.sln` file. To build all services type, go back to the root folder and type `npm run deploy:backend:api`.

## 👥 The Team

[![Contributors](https://contrib.rocks/image?repo=amosproj/amos2024ss04-building-information-enhancer)](https://github.com/amosproj/amos2024ss04-building-information-enhancer/graphs/contributors)
