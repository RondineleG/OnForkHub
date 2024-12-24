FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Configurar variáveis de ambiente
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Instalar dependências necessárias
RUN apt-get update && apt-get install -y \
    curl \
    libicu-dev \
    && rm -rf /var/lib/apt/lists/*

# Instalar Node.js e npm
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    npm install -g npm@latest

# Copiar arquivos do projeto
COPY ["src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj", "src/Presentations/OnForkHub.Web/"]
COPY ["src/Presentations/OnForkHub.Web.Components/OnForkHub.Web.Components.csproj", "src/Presentations/OnForkHub.Web.Components/"]

# Restaurar dependências e publicar
RUN dotnet restore "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" --disable-parallel
COPY . .
RUN dotnet build "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" -c Release --no-restore
RUN dotnet publish "src/Presentations/OnForkHub.Web/OnForkHub.Web.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
RUN ls -la /app/
ENTRYPOINT ["dotnet", "OnForkHub.Web.dll"]
