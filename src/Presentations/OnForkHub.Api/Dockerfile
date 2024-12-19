FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

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
RUN dotnet build "./OnForkHub.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./OnForkHub.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OnForkHub.Api.dll"]
