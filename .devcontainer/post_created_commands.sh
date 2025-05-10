!#/bin/bash
dotnet dev-certs https --trust
dotnet tool restore
dotnet husky install
dotnet restore
dotnet build
dotnet husky run
