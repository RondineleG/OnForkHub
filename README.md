<div align="center">

# Video Sharing Platform with Torrent and CDN Support

<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 600 600">
  <defs>
    <!-- Glow Effect -->
    <filter id="glow">
      <feGaussianBlur stdDeviation="2" result="coloredBlur"/>
      <feFlood flood-color="#4ade80" flood-opacity="0.3" result="glowColor"/>
      <feComposite in="glowColor" in2="coloredBlur" operator="in" result="softGlow"/>
      <feMerge>
        <feMergeNode in="softGlow"/>
        <feMergeNode in="SourceGraphic"/>
      </feMerge>
    </filter>
    <!-- Gradients -->
    <linearGradient id="nodeGradient" x1="0%" y1="0%" x2="100%" y2="100%">
      <stop offset="0%" style="stop-color:#4ade80"/>
      <stop offset="50%" style="stop-color:#22c55e"/>
      <stop offset="100%" style="stop-color:#16a34a"/>
    </linearGradient>
    <linearGradient id="flowGradient" x1="50%" y1="0%" x2="50%" y2="100%">
      <stop offset="0%" style="stop-color:#8b5cf6;stop-opacity:1"/>
      <stop offset="100%" style="stop-color:#6d28d9;stop-opacity:1"/>
    </linearGradient>
  </defs>
  <!-- Outer Rotating Dotted Circle -->
  <g transform="translate(300, 300)" filter="url(#glow)">
    <circle cx="0" cy="0" r="120" fill="none" stroke="#8b5cf6"
            stroke-width="3" stroke-dasharray="6,12" opacity="0.8">
      <animateTransform attributeName="transform"
                        type="rotate"
                        from="0"
                        to="360"
                        dur="10s"
                        repeatCount="indefinite"/>
    </circle>
    <!-- Connection Points -->
    <g>
      <circle cx="120" cy="0" r="12" fill="url(#nodeGradient)">
        <animate attributeName="r" values="12;14;12" dur="5s" repeatCount="indefinite"/>
      </circle>
      <circle cx="85" cy="-85" r="12" fill="url(#nodeGradient)"/>
      <circle cx="0" cy="-120" r="12" fill="url(#nodeGradient)"/>
      <circle cx="-85" cy="-85" r="12" fill="url(#nodeGradient)"/>
      <circle cx="-120" cy="0" r="12" fill="url(#nodeGradient)"/>
      <circle cx="-85" cy="85" r="12" fill="url(#nodeGradient)"/>
      <circle cx="0" cy="120" r="12" fill="url(#nodeGradient)"/>
      <circle cx="85" cy="85" r="12" fill="url(#nodeGradient)"/>
    </g>
    <!-- Central Circle -->
    <circle cx="0" cy="0" r="50" fill="url(#nodeGradient)">
      <animate attributeName="r" values="40;42;40" dur="2s" repeatCount="indefinite"/>
    </circle>
    <!-- Connecting Lines -->
    <path d="M0,0 L120,0 M0,0 L85,-85 M0,0 L0,-120 M0,0 L-85,-85 M0,0 L-120,0 M0,0 L-85,85 M0,0 L0,120 M0,0 L85,85"
          stroke="url(#flowGradient)" stroke-width="3" stroke-dasharray="6" stroke-linecap="round">
      <animate attributeName="stroke-dashoffset" values="0;-48" dur="4s" repeatCount="indefinite"/>
    </path>
    <!-- Play Icon (Moved Last to Appear on Top) -->
    <path d="M-15,-22 L20,0 L-15,22 Z" fill="#fff">
      <animateTransform attributeName="transform"
                        type="scale"
                        values="1;1.1;1"
                        dur="2s"
                        repeatCount="indefinite"/>
    </path>
  </g>
</svg>

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

 ### Development Environment
- **🟥 Windows**: Used for local development

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
