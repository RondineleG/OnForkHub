FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

WORKDIR /src

COPY . .

RUN dotnet restore "./src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" && \
    dotnet publish "./src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

RUN ls -la /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

RUN ls -la /app

ENV ASPNETCORE_ENVIRONMENT=Development \
    ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "OnForkHub.Web.dll"]
