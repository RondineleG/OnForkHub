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

---

## User Stories

### 1. Infrastructure Setup

#### Story 1.1: Set up Azure Cosmos DB for Data Storage
- **Tasks**:
  - [ ] Create a Cosmos DB instance with collections `Users`, `Videos`, `Categories`, and `Terms`.
  - [ ] Configure data structure to support efficient querying and ensure scalability.
  - [ ] Implement security and backup policies for Cosmos DB.

#### Story 1.2: Set up Azure Blob Storage for Video and Torrent Storage
- **Tasks**:
  - [ ] Create a Blob Storage with an organized folder structure to store converted videos and torrent files in batches of up to 100.
  - [ ] Implement access control to ensure data security.
  - [ ] Set up monitoring to alert for errors or performance issues.

### 2. Backend API Development

#### Story 2.1: Implement Video Upload Endpoint
- **Description**: Enable users to upload videos in various formats.
- **Tasks**:
  - [ ] Create the `/upload` endpoint to receive videos and temporarily store them on the server.
  - [ ] Validate video format and size.

#### Story 2.2: Integrate Automatic Moderation with Azure Content Moderator
- **Description**: Automatically moderate videos to ensure compliance with content guidelines.
- **Tasks**:
  - [ ] Set up Azure Content Moderator to process uploaded videos.
  - [ ] Validate content before publishing and store moderation results in Cosmos DB.

#### Story 2.3: Implement Video Conversion Service to MP4
- **Description**: Convert videos to `.mp4` format to ensure compatibility.
- **Tasks**:
  - [ ] Implement automatic conversion of videos to `.mp4`.
  - [ ] Store converted videos in Blob Storage.

#### Story 2.4: Implement Torrent Generation for Converted Videos
- **Description**: Generate torrent files for each converted video and store them in batches.
- **Tasks**:
  - [ ] Generate torrent for each converted video using `MonoTorrent`.
  - [ ] Store the torrent file path in Cosmos DB.
  - [ ] Organize torrents into batches of 100.

### 3. Torrent Seeding with Docker

#### Story 3.1: Configure Docker for Torrent Seeding Batches
- **Description**: Use Docker to serve torrent files, distributing the load.
- **Tasks**:
  - [ ] Create Docker containers to seed torrents.
  - [ ] Organize containers to manage torrents in batches of 100 files.

#### Story 3.2: Implement Scalability with Azure Kubernetes Service (AKS)
- **Description**: Manage Docker container scalability for seeds.
- **Tasks**:
  - [ ] Configure AKS to automatically scale containers based on demand.

### 4. Frontend Development with Blazor WebAssembly

#### Story 4.1: Create Video Upload Interface
- **Description**: User-friendly interface for video upload.
- **Tasks**:
  - [ ] Develop upload page connected to the `/upload` endpoint.
  - [ ] Display progress and feedback to the user during upload.

#### Story 4.2: Implement Approved Video Listing
- **Description**: Display approved videos for viewing.
- **Tasks**:
  - [ ] Create a page to list approved videos.
  - [ ] Update the listing in real time with new videos.

#### Story 4.3: Create Player for Video Playback via Torrent
- **Description**: Player for videos available via torrent with WebTorrent.
- **Tasks**:
  - [ ] Integrate WebTorrent for direct streaming in the browser.
  - [ ] Ensure fallback to CDN for unsupported browsers.

#### Story 4.4: Terms and Conditions Page
- **Description**: Terms of use page requiring acceptance before accessing content.
- **Tasks**:
  - [ ] Create a page displaying the Terms and Conditions.
  - [ ] Prompt the user to accept terms before accessing videos.

---

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
