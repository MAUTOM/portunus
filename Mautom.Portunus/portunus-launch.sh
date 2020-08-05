#!/bin/sh
ASPNETCORE_ENVIRONMENT=Development GNUPGHOME=/home/horvathb/PGP/portunus /usr/bin/authbind --deep /usr/bin/dotnet ./Mautom.Portunus.dll
read