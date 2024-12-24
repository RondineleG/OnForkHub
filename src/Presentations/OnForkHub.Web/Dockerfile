FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Directory.Build.props", "."]
COPY ["src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj", "src/Presentations/OnForkHub.Web/"]
COPY ["src/Presentations/OnForkHub.Web.Components/OnForkHub.Web.Components.csproj", "src/Presentations/OnForkHub.Web.Components/"]
RUN apt-get update && apt-get install -y curl && \
   curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
   apt-get install -y nodejs && \
   npm install -g npm@latest
   
RUN dotnet restore "./src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj"
COPY . .
WORKDIR "/src/src/Presentations/OnForkHub.Web"
RUN dotnet build "./OnForkHub.Web.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "./OnForkHub.Web.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_USE_POLLING_FILE_WATCHER=1 \
    ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "OnForkHub.Web.dll"]
