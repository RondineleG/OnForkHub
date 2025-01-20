<div align="center">

# Video Sharing Platform with Torrent and CDN Support

<img src="docs/assets/logo.svg" width="400" alt="OnForkHub Logo"/>

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Issues Welcome](https://img.shields.io/badge/Issues-welcome-brightgreen.svg)](https://github.com/RondineleG/OnForkHub/issues)
[![Code Analysis](https://github.com/RondineleG/OnForkHub/actions/workflows/qodana-code-quality.yml/badge.svg)](https://github.com/RondineleG/OnForkHub/actions/workflows/qodana-code-quality.yml)
[![Validate Commit Messages](https://github.com/RondineleG/OnForkHub/actions/workflows/validate-commit.yaml/badge.svg)](https://github.com/RondineleG/OnForkHub/actions/workflows/validate-commit.yaml)
[![Build Docker Images](https://github.com/RondineleG/OnForkHub/actions/workflows/gitHub-container-registry.yml/badge.svg?branch=dev&event=push)](https://github.com/RondineleG/OnForkHub/actions/workflows/gitHub-container-registry.yml)

*A distributed video-sharing platform with hybrid P2P and CDN delivery*

</div>

## 🎯 Project Overview

OnForkHub is a cutting-edge video-sharing platform that combines traditional CDN delivery with P2P distribution through WebTorrent. Supporting videos up to 2 minutes, it offers:

- 🚀 Hybrid content delivery (CDN + P2P)
- 📱 Responsive web interface
- 🔒 Secure authentication
- 🎬 Automatic video transcoding
- 📊 Analytics and monitoring

## 🛠 Tech Stack

### Core Technologies

- **🎯 Backend**: .NET 9 with ASP.NET Core
- **🌐 Frontend**: Blazor WebAssembly
- **📦 Storage**: Azure Blob Storage/AWS S3
- **🔄 P2P**: WebTorrent
- **🐳 Containers**: Docker

### Development Tools

- **📝 IDE**: Visual Studio 2022/VS Code
- **🔨 Version Control**: Git + Git Flow
- **🎨 Code Style**: CSharpier
- **🐶 Git Hooks**: Husky
- **🔍 CI/CD**: GitHub Actions

## 📋 Prerequisites

### Required Software

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Git** - [Download](https://git-scm.com/)
- **GitHub CLI** - [Download](https://cli.github.com/)
- **Node.js** - [Download](https://nodejs.org/)
- **Docker** - [Download](https://www.docker.com/)

### Recommended Extensions

- **Visual Studio**
  - GitFlow for Visual Studio 2022
  - CSharpier

- **VS Code**
  - C# Dev Kit
  - GitLens
  - Git Flow
  - CSharpier

  ## 🚀 Deployment Guide
### VPS Specifications
The project has been successfully deployed and tested on a minimal VPS configuration:
- **Type**: Virtual Machine
- **Size**: VPS 1/1/10
- **CPU**: 1 vCore
- **RAM**: 1 GB
- **Storage**: 10 GB NVMe SSD

### Public Access
Development  environment  deployment is available at:
- **Frontend**: http://20.119.85.209/
- **API Documentation**: http://20.119.85.209:9000/swagger/index.html

### Docker Setup
The deployment uses Docker with a reverse proxy configuration:
- Frontend accessible on port 80
- API accessible on port 9000

#### Configuration Files
- `proxy.yml`: Nginx reverse proxy setup
- `custom.conf`: Nginx configuration
- `services.yml`: Application services configuration


## 🚀 Starting to develop

### First-Time Setup

1. **Install and Configure GitHub CLI**

```bash
# Download and install GitHub CLI from https://cli.github.com/

# Then configure:
gh auth login
gh auth status  # Verify authentication
```

2. **Clone and Setup Repository**

```bash
# Clone using GitHub CLI
gh repo clone RondineleG/OnForkHub
cd OnForkHub

# Build and configure project (includes .NET tools and Husky setup)
dotnet build && dotnet husky run
```

3. **Setup Local Environment**

```bash
# Install dependencies
dotnet build && dotnet husky run

# Build solution
dotnet build

# Setup development certificates
dotnet dev-certs https --trust
```
### Dev Container setup
- If you are using **Dev Container** extension for VS Code or Visual Studio, just reopen the folder inside the container

- If you **aren't** using Dev Container extension:
1.
```
./.devcontainer/devcontainer_setup.sh
```
*OR*
```
docker build -t on-fork-hub -f .devcontainer/Dockerfile .
docker run --rm -it -v $(pwd):/app -w /app on-fork-hub bash
```
2.
```
# inside the container
./.devcontainer/post_created_commands.sh
```
*OR*
```
dotnet dev-certs https --trust
dotnet tool restore
dotnet husky install
dotnet restore
dotnet build
dotnet husky run
```
### Development Workflow

#### 1. Branch Management (Git Flow)

We follow Git Flow with kebab-case naming convention for all branches (using lowercase letters and hyphens).
 This ensures:

- Better readability in URLs and command line
- Compatibility across different operating systems
- Consistency with Git Flow conventions
- Avoidance of case-sensitivity issues

```bash
# Branch Types and Naming Convention
main        # Production branch
dev         # Development branch
feature/    # New features (e.g., feature/add-user-auth)
hotfix/     # Production fixes (e.g., hotfix/fix-login-bug)
release/    # Release preparation (e.g., release/v1.2.0)
bugfix/     # Non-urgent fixes (e.g., bugfix/improve-error-handling)

# Start new feature
git flow feature start add-user-auth

# Publish feature
git flow feature publish add-user-auth

# Complete feature and create an automatic Pull Request
git flow feature finish add-user-auth

# Start hotfix
git flow hotfix start fix-login-bug

# Start release
git flow release start v1.2.0
```

#### 2. Commit Standards

```bash
# Structure
<type>(<scope>): <description>

[optional body]

[optional footer(s)]

# Example
feat(auth): implement multi-factor authentication

- Add SMS verification
- Implement authenticator app support
- Add backup codes generation

Closes #123
```

#### 3. Local Development

```bash
# Run API
cd src/Presentations/OnForkHub.Api
dotnet watch run

# Run WebUI
cd src/Presentations/OnForkHub.Web
dotnet watch run
```

#### 4. Git Aliases

After setting up the project, you will have access to convenient Git aliases. They become available after restarting your terminal or running `. $PROFILE` in your terminal:

```bash
# Git Aliases Quick Reference
gs    # git status -sb (shows clean branch status)
ga    # git add --all (stages all changes)
gc -m "message"  # git commit with message
gps   # git push
gpl   # git pull
gf    # git fetch

# Branch Management
gb    # git branch
gco   # git checkout

# Log & History
gl -n  # git log with nice formatting (e.g. gl -20 for last 20 commits)
gt -n  # git tree view (e.g. gt -10 for last 10 commits)
gd     # git diff (shows uncommitted changes)
gr     # git remote -v (shows remote repositories)
```

#### 5. CLI Tool (dtn)

The `dtn` CLI tool is a development utility for OnForkHub that helps with package management and pull request creation.

#### Installation

The CLI tool is automatically installed when you build the project. To manually install or update:

```bash
cd src/Shared/OnForkHub.Scripts
dotnet pack
dotnet tool update -g --add-source ./nupkg OnForkHub.Scripts
```

#### Usage

```bash
dtn <command> [options]
```

#### Commands

- **Package Management**
  ```bash
  # Install package directly
  dtn -i <package> [-v version]    # Example: dtn -i Serilog -v 3.*

  # Search and install packages interactively
  dtn -s [searchTerm]             # Example: dtn -s Newtonsoft
  ```

- **Pull Requests**
  ```bash
  # Create pull request
  dtn -p                          # Creates PR from current feature branch to dev
  ```

- **Help**
  ```bash
  # Show help
  dtn -h                          # Display available commands and examples
  ```

#### Examples

```bash
# Install specific package version
dtn -i Microsoft.EntityFrameworkCore -v 9.0.0

# Search for logging packages
dtn -s Serilog

# Interactive package selection example:
Found packages:
[INFO] 0: Serilog (Latest: 4.1.0)
[INFO] 1: Serilog.Sinks.File (Latest: 6.0.0)
# Enter selection: 0 4.*, 1 6.*

# Create pull request for current feature branch
dtn -p
```

#### Notes

- Package installations are always targeted to `src/Shared/OnForkHub.Dependencies/OnForkHub.Dependencies.csproj`
- The `-p` command requires a clean git working tree
- Version patterns (like `3.*`) are supported for package installation

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Setting Up for Development

1. Fork the repository
2. Create your feature branch (`git flow feature start FeatureName`)
3. Commit your changes using [Conventional Commits](https://www.conventionalcommits.org/)
4. Push to the branch (`git flow feature publish FeatureName`)
5. Open a Pull Request in GitHub manually, or use (`git flow feature finish FeatureName`) to create an automatic Pull Request

## ❓ Troubleshooting

### Common Issues & Solutions

#### GitHub CLI Issues

```bash
# Reset authentication
gh auth logout
gh auth login

# Verify status
gh auth status
```

#### Git Flow Issues

```bash
# Reinitialize Git Flow
git flow init -f

# Check configuration
git config --list | findstr "gitflow"
```

#### Build Issues

```bash
# Clean solution
dotnet clean

# Clear NuGet cache
dotnet nuget locals all --clear

# Rebuild
dotnet build --no-incremental
```

## 📬 Support

- 📧 Email: rondineleg@gmail.com
- 💬 Telegram: [Join Our Server](https://t.me/OnForkHub)
- 🐛 Issues: [GitHub Issues](https://github.com/RondineleG/OnForkHub/issues)

## 📜 License

Copyright © 2024 OnForkHub - Released under the [MIT License](LICENSE).

