FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

RUN apt-get update && apt-get install -y curl && \
   curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
   apt-get install -y nodejs && \
   npm install -g npm@latest

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj"
RUN dotnet publish "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN ls -la /app/
ENTRYPOINT ["dotnet", "OnForkHub.Web.dll"]
