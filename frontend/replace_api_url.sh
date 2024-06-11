#!/bin/bash

# Variables for the new values
NEW_API_GATEWAY_HOST="new_host_value"
NEW_API_GATEWAY_PORT="new_port_value"

# Directory containing the .js files
DIRECTORY="."

# Find and replace the API_GATEWAY_HOST and API_GATEWAY_PORT in all .js files
find "$DIRECTORY" -type f -name "*.js" -exec sed -i \
    -e "s/API_GATEWAY_HOST/$NEW_API_GATEWAY_HOST/g" \
    -e "s/API_GATEWAY_PORT/$NEW_API_GATEWAY_PORT/g" {} \;

echo "API Gateway URL replacement completed, starting Apache HTTPD server..."
httpd-foreground