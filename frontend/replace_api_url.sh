#!/bin/bash

# Variables for the new values
NEW_API_GATEWAY_HOST=$1
echo "Host to replace: $1"
NEW_API_GATEWAY_PORT=$2
echo "Port to replace: $2"

# Directory containing the .js files
DIRECTORY="."

# Find and replace the API_GATEWAY_HOST and API_GATEWAY_PORT in all .js files
find "$DIRECTORY" -type f -name "*.js" -exec sed -i \
    -e "s|API_GATEWAY_HOST|$NEW_API_GATEWAY_HOST|g" \
    -e "s|API_GATEWAY_PORT|$NEW_API_GATEWAY_PORT|g" {} \;

echo "API Gateway URL replacement completed, starting Apache HTTPD server..."
httpd-foreground