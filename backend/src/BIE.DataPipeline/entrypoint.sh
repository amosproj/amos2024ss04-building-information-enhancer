#!/bin/bash

sleep 10s

echo "Inserting datasets..."

common_directory="./yaml/common"
production_directory="./yaml/production"
development_directory="./yaml/development"
yamlfiles=$(find "$common_directory" -type f -name "*.yaml")
excludeFiles=("unitTest.yaml" "unitTestShort.yaml")

if [ "$ENVIRONMENT_STAGE" = "production" ]; then
    echo "Production environment detected, loading all datasets..."
    production_yamlfiles=$(find "$production_directory" -type f -name "*.yaml")
    yamlfiles="$yamlfiles $production_yamlfiles"
else
    echo "Development environment detected, loading common and development datasets..."
    development_yamlfiles=$(find "$development_directory" -type f -name "*.yaml")
    yamlfiles="$yamlfiles $development_yamlfiles"
fi

echo "--------------------------------------------------------------"
# Check if yaml_files is not empty
if [ -n "$yamlfiles" ]; then
    # Loop through each file in yaml_files
    for file in $yamlfiles; do
        filename=$(basename "$file")
        
        # Check if the filename is in the exclude list
        if [[ ! " ${excludeFiles[@]} " =~ " ${filename} " ]]; then
            dotnet BIE.DataPipeline.dll $file
        fi
    done
else
    echo "No YAML files found in $common_directory, $production_directory, or $development_directory"
fi

echo "Data insertion completed! Data Pipeline has finished."