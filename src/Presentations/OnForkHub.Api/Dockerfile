FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

COPY ["global.json", "."]
COPY ["Directory.Build.props", "."]
COPY ["src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj", "src/Presentations/OnForkHub.Web/"]
COPY ["src/Presentations/OnForkHub.Web.Components/OnForkHub.Web.Components.csproj", "src/Presentations/OnForkHub.Web.Components/"]

RUN dotnet nuget locals all --clear
RUN dotnet restore "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" --verbosity detailed

COPY . .
WORKDIR "/src/src/Presentations/OnForkHub.Web"
RUN npm install --prefix wwwroot/npm
RUN dotnet publish "OnForkHub.Web.csproj" -c Debug -o /app/publish /p:UseAppHost=false --verbosity detailed

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_USE_POLLING_FILE_WATCHER=1 \
    ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "OnForkHub.Web.dll"]
