#!/usr/bin/env sh
find '/usr/local/apache2/htdocs' -name '*.js' -exec sed -i -e 's,API_GATEWAY_HOST,'"$API_GATEWAY_HOST"',g' {} \;
find '/usr/local/apache2/htdocs' -name '*.js' -exec sed -i -e 's,API_GATEWAY_PORT,'"$API_GATEWAY_PORT"',g' {} \;
httpd-foreground
