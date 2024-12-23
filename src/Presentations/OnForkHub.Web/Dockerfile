FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

COPY ["Directory.Build.props", "."]
COPY ["src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj", "src/Presentations/OnForkHub.Web/"]
COPY ["src/Core/OnForkHub.Application/OnForkHub.Application.csproj", "src/Core/OnForkHub.Application/"]
COPY ["src/Core/OnForkHub.Core/OnForkHub.Core.csproj", "src/Core/OnForkHub.Core/"]
COPY ["src/Shared/OnForkHub.Abstractions/OnForkHub.Abstractions.csproj", "src/Shared/OnForkHub.Abstractions/"]
COPY ["src/Shared/OnForkHub.Dependencies/OnForkHub.Dependencies.csproj", "src/Shared/OnForkHub.Dependencies/"]

RUN dotnet restore "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj"

COPY . .

RUN ls -la /src/src/Presentations/OnForkHub.Web

WORKDIR "/src/src/Presentations/OnForkHub.Web"
RUN dotnet build "OnForkHub.Web.csproj" -c Release --no-restore -v d
RUN dotnet publish "OnForkHub.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false -v d

RUN ls -la /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

RUN ls -la /app && \
    echo "Content of app directory:" && \
    find /app -type f -name "*.dll" -exec ls -la {} \;

ENV ASPNETCORE_ENVIRONMENT=Development \
    DOTNET_URLS=http://+:80

ENTRYPOINT ["dotnet", "OnForkHub.Web.dll"]
