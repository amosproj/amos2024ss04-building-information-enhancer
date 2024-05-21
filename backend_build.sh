#!/bin/bash

# Check if the runtime argument is provided
if [ -z "$1" ]; then
  echo "Usage: $0 <runtime>"
  echo "Example: $0 linux-x64"
  exit 1
fi

RUNTIME=$1

echo "Building DataPipeline"
echo "---------------------"
cd backend/src/BIE.DataPipeline/bin && rm -rf build
cd - 
cd backend/src/BIE.DataPipeline || exit
dotnet publish BIE.DataPipeline.csproj -c Release --self-contained true -o ./bin/build -r "$RUNTIME" 

echo "Building API"
echo "------------"
cd -
cd backend/src/BIE.Core/BIE.Core.API/bin && rm -rf build
cd -
cd backend/src/BIE.Core/BIE.Core.API || exit
dotnet publish BIE.Core.API.csproj -c Release --self-contained true -o ./bin/build -r "$RUNTIME"
