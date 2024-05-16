echo Building DataPipeline
echo ---------------------
cd backend/src/BIE.DataPipeline/bin
rmdir /s /q build
cd ..
dotnet publish BIE.DataPipeline.csproj -c Release --self-contained true -o ./bin/build

echo Building API
echo ------------
cd backend/src/BIE.Core/BIE.Core.API/bin
rmdir /s /q build
cd ..
dotnet publish BIE.DataPipeline.csproj -c Release --self-contained true -o ./bin/build