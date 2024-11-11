# Video Sharing Platform with Torrent and CDN Support

This repository contains a short video-sharing platform (up to 2 minutes) with hybrid content distribution using torrents and CDN. The application is built with a C# API (ASP.NET Core), Blazor WebAssembly frontend, WebTorrent for P2P streaming in the browser, and Docker support to orchestrate seed containers.

## Project Overview

This project aims to create a scalable and distributed platform for video sharing, where users can upload videos in various formats, which are then converted to torrents and made available via CDN and WebTorrent. This enables fast content delivery through CDN while leveraging the P2P network to reduce bandwidth costs and enhance distribution.

## Technologies Used

- **Back-end**: C# with ASP.NET Core for RESTful API
- **Front-end**: Blazor WebAssembly
- **Storage**: Azure Blob Storage or AWS S3
- **Serverless Functions**: Azure Functions or AWS Lambda
- **P2P Streaming**: WebTorrent for streaming torrents directly in the browser
- **Containers**: Docker for seed containers
- **Torrent Conversion**: MonoTorrent (C# library for torrent management)
- **CDN**: Configured with P2P support (options: Azure CDN, Peer5, Streamroot, or CDNBye)
- **Database**: Azure Cosmos DB for video metadata and moderation logs
- **Git Flow**: Branch management

 ### Development Environment
- **🟥 Windows**: Used for local development

## Environment Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) SDK for development
- [WebTorrent](https://webtorrent.io/) for WebTorrent packages and dependencies
- [Docker](https://www.docker.com/) for seed containers and orchestration

### Initial Setup

1. Clone this repository:
   ```bash
   git clone https://github.com/RondineleG/OnForkHub.git
   cd OnForkHub
    dotnet build && dotnet husky run
