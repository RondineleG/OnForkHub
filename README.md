<div align="center">

# Video Sharing Platform with Torrent and CDN Support

<img src="docs/assets/logo.svg" width="400" alt="OnForkHub Logo"/>

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](http://makeapullrequest.com)

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

## 🚀 Getting Started

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

### Development Workflow

#### 1. Branch Management (Git Flow)

```bash
# Start new feature
git flow feature start FeatureName

# Publish feature
git flow feature publish FeatureName

# Complete feature and create an automatic Pull Request
git flow feature finish FeatureName
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
# Core Commands
gs  # git status -sb
ga  # git add --all
gc  # git commit -ev
gps # git push
gpl # git pull
gf  # git fetch

# Branch Management
gb  # git branch
gco # git checkout

# Viewing Changes
gl  # git log with nice formatting
gt  # git log --graph (tree view)
gd  # git diff
gr  # git remote -v
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

## 📜 License

Copyright © 2024 OnForkHub - Released under the [MIT License](LICENSE).

## 📬 Support

- 📧 Email: rondineleg@gmail.com
- 💬 Telegram: [Join Our Server](https://t.me/OnForkHub)
- 🐛 Issues: [GitHub Issues](https://github.com/RondineleG/OnForkHub/issues)
