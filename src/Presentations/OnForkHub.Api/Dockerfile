FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["src/Presentations/OnForkHub.Api/OnForkHub.Api.csproj", "src/Presentations/OnForkHub.Api/"]
COPY ["src/Core/OnForkHub.Application/OnForkHub.Application.csproj", "src/Core/OnForkHub.Application/"]
COPY ["src/Core/OnForkHub.Core/OnForkHub.Core.csproj", "src/Core/OnForkHub.Core/"]
COPY ["src/Shared/OnForkHub.Abstractions/OnForkHub.Abstractions.csproj", "src/Shared/OnForkHub.Abstractions/"]
COPY ["src/Shared/OnForkHub.Dependencies/OnForkHub.Dependencies.csproj", "src/Shared/OnForkHub.Dependencies/"]
COPY ["src/Persistence/OnForkHub.Persistence/OnForkHub.Persistence.csproj", "src/Persistence/OnForkHub.Persistence/"]

RUN dotnet restore "./src/Presentations/OnForkHub.Api/OnForkHub.Api.csproj"

COPY . .

WORKDIR "/src/src/Presentations/OnForkHub.Api"

RUN dotnet build "./OnForkHub.Api.csproj" -c Debug -o /app/build

FROM build AS publish
RUN dotnet publish "./OnForkHub.Api.csproj" -c Debug -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_USE_POLLING_FILE_WATCHER=1 \
    ASPNETCORE_URLS=https://+:443;http://+:80

RUN dotnet dev-certs https --clean && \
    dotnet dev-certs https --trust && \
    mkdir -p /https && \
    dotnet dev-certs https -ep /https/aspnetapp.pfx -p password

ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=password

ENTRYPOINT ["dotnet", "OnForkHub.Api.dll"]
