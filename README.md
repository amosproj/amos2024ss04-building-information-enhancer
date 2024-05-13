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

Welcome to the official repository for the `CODE.ING` group, developing an open-source `Building Information Enhancer` software for the `BUILD.ING` partner and the `AMOS 2024` project. Together, we create a Building information system for potential energy savings. To read about the architecture of our service visit our GitHub [wiki](https://github.com/Corgam/SS23_ADSP_TCF/wiki).

## 🚀 Setup & Deployment

This project is integrated with a full CI/CD pipeline, thus deployed automatically on the web. However, it is possible to deploy a production-ready system on a local machine using Docker or deploy each part of the system separately for further development. For further information on the `CI/CD` pipeline visit our GitHub [wiki](https://github.com/Corgam/SS23_ADSP_TCF/wiki) documentation.

## 📦 Deployment using Docker

To deploy the project on your local machine using Docker Engine, follow the instructions below:

1. Clone this repository (https://github.com/amosproj/amos2024ss04-building-information-enhancer).
2. Before deploying the project be sure you have installed the [Node.js](https://nodejs.org/en) ( >= 20.12.2) and a running [Docker Deamon](https://docs.docker.com/engine/install/).
3. Run the `npm run deploy` command in the root folder of the repository.
4. Connect to the Frontend located at port 80, usually `localhost:80`.

## 💻 Developement Deployment

To deploy one or more services of the system separately, follow the instructions below:

1. Clone this repository (https://github.com/amosproj/amos2024ss04-building-information-enhancer).
2. Before deploying the project be sure you have installed the [Node.js](https://nodejs.org/en) ( >= 20.12.2) and a running [Docker Deamon](https://docs.docker.com/engine/install/).
3. Run the `npm run setup` command to install all necessary packages.
4. To deploy the frontend use the `npm run deploy:dev:frontend` command, or to deploy the backend use the `npm run deploy:dev:backend` command.

## 👥 The Team

[![Contributors](https://contrib.rocks/image?repo=amosproj/amos2024ss04-building-information-enhancer)](https://github.com/amosproj/amos2024ss04-building-information-enhancer/graphs/contributors)
