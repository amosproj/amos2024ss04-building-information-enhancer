#!/bin/bash

sleep 10s

echo "insert data sets"

directory="./yaml"
yamlfiles=$(find "$directory" -type f -name "*.yaml")
excludeFiles=("unitTest.yaml" "evChargingStationNoGeography.yaml")

# Check if yaml_files is not empty
if [ -n "$yamlfiles" ]; then
    # Loop through each file in yaml_files
		for file in $yamlfiles; do
		filename=$(basename "$file")
		
		# Check if the filename is in the exclude list
		if [[ ! " ${excludeFiles[@]} " =~ " ${filename} " ]]; then			
			echo "run $file"
			dotnet BIE.DataPipeline.dll $file
		fi
    done
else
    echo "No YAML files found in $directory"
fi

echo "insertion completed"
