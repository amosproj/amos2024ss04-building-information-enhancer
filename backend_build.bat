@echo off
echo Building DataPipeline
echo ---------------------
cd backend/src/BIE.DataPipeline/bin
rmdir /s /q build
cd %~dp0
cd backend/src/BIE.DataPipeline
dotnet publish BIE.DataPipeline.csproj -c Release --self-contained true -o ./bin/build -r win-x64


echo Building API
echo ------------
cd %~dp0
cd backend/src/BIE.Core/BIE.Core.API/bin
rmdir /s /q build
cd %~dp0
cd backend/src/BIE.Core/BIE.Core.API
dotnet publish BIE.Core.API.csproj -c Release --self-contained true -o ./bin/build -r win-x64