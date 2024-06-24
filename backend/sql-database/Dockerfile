FROM mcr.microsoft.com/mssql/server:2019-latest
ENV SA_PASSWORD MyPass@1234
ENV ACCEPT_EULA Y

USER root

RUN apt-get update && apt-get install -y dos2unix


COPY ./ /
COPY ./scripts/CreateDB.sql /setup.sql

RUN dos2unix /entrypoint.sh
RUN dos2unix /run-script.sh

RUN chmod +x /run-script.sh

ENTRYPOINT ["/bin/bash","entrypoint.sh"]
CMD ["/opt/mssql/bin/sqlservr"]


