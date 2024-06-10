#!/bin/sh

# Inject environment variables at runtime
import-meta-env -x /usr/src/app/.env.example

# Start the Apache HTTP server
httpd-foreground
