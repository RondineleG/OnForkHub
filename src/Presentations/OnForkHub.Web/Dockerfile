FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

RUN apt-get update && apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

COPY ["src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj", "src/Presentations/OnForkHub.Web/"]
COPY ["src/Presentations/OnForkHub.Web.Components/OnForkHub.Web.Components.csproj", "src/Presentations/OnForkHub.Web.Components/"]

RUN dotnet restore "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj"

COPY . .

RUN dotnet publish "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN ls -la /app/
ENTRYPOINT ["dotnet", "OnForkHub.Web.dll"]
