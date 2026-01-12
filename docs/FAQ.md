# ❓ Frequently Asked Questions (FAQ)

Common questions and answers about OnForkHub development and usage.

---

## Table of Contents

- [General Questions](#general-questions)
- [Setup & Installation](#setup--installation)
- [Development](#development)
- [Testing](#testing)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

---

## General Questions

### Q: What is OnForkHub?

**A:** OnForkHub is a distributed video-sharing platform that combines CDN (Content Delivery Network) with P2P (Peer-to-Peer) distribution through WebTorrent. It's built with .NET 9 and provides a modern, scalable solution for video sharing and streaming.

### Q: What technologies does OnForkHub use?

**A:** OnForkHub uses:
- **Backend:** .NET 9 with ASP.NET Core
- **Frontend:** Blazor WebAssembly
- **Databases:** RavenDB, CosmosDB, SQL Server
- **Storage:** Azure Blob Storage or AWS S3
- **P2P:** WebTorrent for peer-to-peer distribution
- **Containers:** Docker for deployment

### Q: Can I contribute to OnForkHub?

**A:** Yes! We welcome contributions from the community. Please read [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines and the contribution process.

### Q: Is OnForkHub open source?

**A:** Yes, OnForkHub is released under the MIT License. See [LICENSE](LICENSE) for details.

### Q: How do I report a bug?

**A:** Please use GitHub Issues. Include:
- Steps to reproduce
- Expected vs actual behavior
- Environment details (OS, .NET version, etc.)
- Screenshots or logs if applicable

### Q: How can I request a feature?

**A:** Create an issue on GitHub with:
- Clear feature description
- Use case/motivation
- Proposed implementation (if any)
- Alternative solutions

---

## Setup & Installation

### Q: What are the minimum system requirements?

**A:** 
- .NET 9 SDK or later
- Git 2.30+
- 4GB RAM (8GB recommended)
- 2GB free disk space
- Windows/Mac/Linux

### Q: How do I install OnForkHub locally?

**A:**
```bash
# Clone repository
gh repo clone RondineleG/OnForkHub
cd OnForkHub

# Setup environment
dotnet build
dotnet husky install

# Trust HTTPS certificate
dotnet dev-certs https --trust

# Run tests
dotnet test
```

### Q: What if I don't have GitHub CLI installed?

**A:**
```bash
# Install from https://cli.github.com/
# Or clone directly with Git:
git clone https://github.com/RondineleG/OnForkHub.git
cd OnForkHub
```

### Q: How do I setup Docker for development?

**A:**
```bash
# Build Docker image
docker build -t on-fork-hub -f .devcontainer/Dockerfile .

# Run container
docker run --rm -it -v $(pwd):/app -w /app on-fork-hub bash

# Inside container, run post-setup
./.devcontainer/post_created_commands.sh
```

### Q: Where should I install the development certificate?

**A:**
```bash
# For Windows/Mac
dotnet dev-certs https --trust

# For Linux (handled automatically in Docker)
# Use the certificate path in docker-compose configuration
```

---

## Development

### Q: How do I run the project locally?

**A:**
```bash
# Terminal 1 - API
cd src/Presentations/OnForkHub.Api
dotnet watch run

# Terminal 2 - Web UI
cd src/Presentations/OnForkHub.Web
dotnet watch run

# Access at:
# API: https://localhost:5001
# Web: https://localhost:5000
```

### Q: What's the recommended Git workflow?

**A:** We use Git Flow:
```bash
# Start feature
git flow feature start feature-name

# Make changes and commit
git commit -m "feat(scope): description"

# Finish feature (creates PR)
git flow feature finish feature-name

# Or use the CLI tool
dtn -p  # Create pull request
```

### Q: How do I create a proper commit message?

**A:** Follow Conventional Commits format:
```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

Examples:
```
feat(auth): add JWT authentication
fix(api): resolve null pointer exception
docs(readme): expand troubleshooting section
test(core): add validation tests
```

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

### Q: Can I use Visual Studio instead of VS Code?

**A:** Yes! Visual Studio 2022 is fully supported. Install:
- C# development workload
- ASP.NET and web development workload
- Optional: GitFlow extension

### Q: How do I debug the application?

**A:**
```bash
# Visual Studio
# F5 or Debug -> Start Debugging

# VS Code
# F5 or Run -> Start Debugging

# Command line
dotnet run --configuration Debug
# Then attach debugger as needed
```

### Q: What's the folder structure?

**A:**
```
OnForkHub/
├── src/
│   ├── Core/           # Business logic
│   ├── Infrastructure/ # Data access
│   ├── Presentations/  # UI (Web, API, App)
│   └── Shared/         # Shared utilities
├── test/               # Test projects
├── docs/               # Documentation
└── scripts/            # Build/utility scripts
```

---

## Testing

### Q: How do I run tests?

**A:**
```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=UserServiceTests"

# Run with code coverage
dotnet test /p:CollectCoverage=true

# Verbose output
dotnet test --verbosity detailed
```

### Q: What's the expected test coverage?

**A:** We aim for 80%+ code coverage. Focus on:
- Happy paths
- Error cases
- Edge cases
- Integration points

### Q: How do I write a unit test?

**A:**
```csharp
[TestClass]
public class CalculatorTests
{
    [TestMethod]
    [DisplayName("Should add two numbers correctly")]
    public void Add_TwoNumbers_ReturnsCorrectSum()
    {
        // Arrange
        var calculator = new Calculator();
        
        // Act
        var result = calculator.Add(2, 3);
        
        // Assert
        result.Should().Be(5);
    }
}
```

### Q: What testing framework should I use?

**A:**
- **xUnit/MSTest:** Test framework
- **NSubstitute:** Mocking library
- **FluentAssertions:** Assertion library

### Q: How do I mock dependencies?

**A:**
```csharp
var mockRepository = Substitute.For<IRepository>();
mockRepository.GetById(Arg.Any<int>()).Returns(expectedData);

var service = new Service(mockRepository);
var result = service.GetData(1);

result.Should().Be(expectedData);
```

---

## Deployment

### Q: How do I deploy to production?

**A:** See deployment guide:
```bash
# Frontend
cd src/Presentations/OnForkHub.Web
dotnet publish -c Release

# API
cd src/Presentations/OnForkHub.Api
dotnet publish -c Release

# Use Docker
docker build -t onforkhub:latest .
docker run -d -p 80:80 -p 5000:5000 onforkhub:latest
```

### Q: What's the deployment architecture?

**A:** OnForkHub uses:
- Docker containers
- Nginx reverse proxy
- Database services (RavenDB, CosmosDB, SQL)
- Cloud storage (Azure Blob/AWS S3)

### Q: How do I configure environment variables?

**A:**
```bash
# Create .env file
DATABASE_CONNECTION=your_connection_string
JWT_SECRET=your_secret_key
STORAGE_ACCOUNT_KEY=your_storage_key

# Or use Docker environment
docker run -e DATABASE_CONNECTION=... onforkhub
```

### Q: How do I setup Azure infrastructure?

**A:** See [ARCHITECTURE.md](docs/ARCHITECTURE.md) for details on:
- App Service
- Key Vault
- Application Insights
- Azure CDN
- Storage Accounts

---

## Troubleshooting

### Q: Build fails with "tsc is not recognized"

**A:**
```bash
npm install -g typescript
npm install --save-dev typescript
cd src/Presentations/OnForkHub.Web
npm install
dotnet build
```

### Q: Tests timeout

**A:**
```bash
# Increase timeout
dotnet test --no-build /p:TestTimeout=60000

# Run sequentially
dotnet test --no-build --parallel 0

# Run specific test
dotnet test --no-build --filter "TestName"
```

### Q: Database connection fails

**A:**
```bash
# Check connection string
cat appsettings.json | grep -i "connection"

# Verify database is running
docker ps | grep database

# Test connection
dotnet ef dbcontext info

# Apply migrations
dotnet ef database update
```

### Q: Git Flow commands don't work

**A:**
```bash
# Initialize Git Flow
git flow init -d

# Or configure manually
git config --local gitflow.branch.master main
git config --local gitflow.branch.develop dev

# Verify
git config --local --list | grep gitflow
```

### Q: Docker container won't start

**A:**
```bash
# Check logs
docker logs container-name

# Check if port is in use
netstat -ano | findstr :80

# Remove dangling containers/images
docker system prune

# Rebuild
docker build --no-cache -t onforkhub .
```

### Q: Performance is slow

**A:**
```bash
# Profile application
dotnet run --configuration Release

# Check database queries
# Enable SQL logging in appsettings.json

# Use Release configuration
dotnet build -c Release

# Analyze memory usage
dotnet run | grep -i memory
```

### Q: Pull request creation fails

**A:**
```bash
# Ensure clean working tree
git status

# Commit all changes
git add .
git commit -m "feat: your changes"

# Update from remote
git fetch origin
git rebase origin/dev

# Try again
dtn -p
# Or
gh pr create --base dev
```

---

## More Help

- **Documentation:** Check `docs/` directory
- **GitHub Issues:** https://github.com/RondineleG/OnForkHub/issues
- **Discussions:** GitHub Discussions tab
- **Telegram:** https://t.me/OnForkHub
- **Email:** rondineleg@gmail.com

---

*Last Updated: 2025-11-07*
*Still have questions? Create an issue or reach out!*
