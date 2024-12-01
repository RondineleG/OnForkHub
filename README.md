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
- **Git Flow**: Branch management with Conventional Commits
- **Code Quality**: CSharpier for formatting, Husky for Git hooks

### Development Environment
- **🟥 Windows**: Used for local development
- **🔷 VS Code And Visual Studio**: Recommended IDE
- **🔨 Git Flow**: Branch management
- **📝 Conventional Commits**: Commit message standard
- **🎨 CSharpier**: Code formatting
- **🐶 Husky**: Git hooks

## Prerequisites

Before you begin, ensure you have the following installed:
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Git](https://git-scm.com/)
- [GitHub CLI (gh)](https://cli.github.com/)
- [Node.js](https://nodejs.org/) 
- [Docker](https://www.docker.com/)
- [WebTorrent](https://webtorrent.io/)

## Installation

1. Clone and setup the repository:
```bash
git clone https://github.com/RondineleG/OnForkHub.git
cd OnForkHub
```

2. Build and setup hooks:
```bash
dotnet build && dotnet husky run
```

3. Install GitHub CLI:
```bash
# Windows (via winget)
winget install GitHub.cli

gh auth login

```

## Development Workflow

### Git Flow and Commits

We use Git Flow with Conventional Commits for development:

1. Start a feature:
```bash
git flow feature start FeatureName
```

2. Make changes and commit using Conventional Commits pattern:
```bash
# Example commits:
feat(auth): implement JWT authentication
fix(api): resolve null reference exception
docs: update API documentation
```

3. Publish a feature:
```bash
git flow feature publish FeatureName
```

4. Finish the feature:
```bash
git flow feature finish FeatureName
```

### Commit Message Format 

```
feat(auth): add JWT authentication system

Added comprehensive JWT implementation:
- Token generation and validation
- Refresh token mechanism
- Role-based authorization
- Secure cookie handling

Closes #123
Breaking-Change: Previous token format is no longer supported
```

#### Types per Branch

- **feature/***
  - `feat:` - New features
  - `refactor:` - Code refactoring
  - `fix:` - Bug fixes
  - `test:` - Tests
  - `docs:` - Documentation
  - `style:` - Code style

- **hotfix/***
  - `fix:` - Bug fixes
  - `hotfix:` - Critical fixes
  - `perf:` - Performance

- **bugfix/***
   - `fix:` - Bug fixes
  - `hotfix:` - Critical fixes
  - `perf:` - Performance

- **release/***
  - `release:` - Release changes
  - `chore:` - Maintenance
  - `docs:` - Documentation

### Code Quality

- **Formatting**: CSharpier automatically formats code in pre-commit
```bash
dotnet csharpier .
```

- **Git Hooks**: 
  - pre-commit: Format check
  - commit-msg: Message validation
  - pre-push: Test execution
  - post-checkout: Setup hooks in checkout
  - pre-flow-feature-finish: PR creation

## Contribution Guide

1. Create a feature branch using Git Flow
2. Make changes following our coding standards
3. Commit using Conventional Commits format
4. Ensure all tests pass
5. Finish feature (PR created automatically)
6. Address review comments

## Troubleshooting

### Common Issues

1. **PR Creation Fails**:
   - Verify GitHub CLI auth: `gh auth status`
   - Check hook logs
   - Create PR manually if needed

2. **Commit Rejected**:
   - Verify branch type
   - Check commit format
   - Follow error message examples

3. **Format Issues**:
   - Run `dotnet csharpier .`
   - Fix syntax errors
   - Commit formatted files

## License

This project is licensed under [MIT License](LICENSE).