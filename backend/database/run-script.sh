#!/bin/bash

/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P MyPass@1234 -d master -i setup.sql
#/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P MyPass@1234 -d BIEDB -i create.sql


