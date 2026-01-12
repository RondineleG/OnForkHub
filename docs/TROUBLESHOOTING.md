# 🔧 Troubleshooting Guide

Comprehensive guide for resolving common issues when developing and running the OnForkHub project.

---

## Table of Contents

- [Build Issues](#build-issues)
- [GitHub CLI Issues](#github-cli-issues)
- [Git Flow Issues](#git-flow-issues)
- [Docker Issues](#docker-issues)
- [Database Issues](#database-issues)
- [Testing Issues](#testing-issues)
- [Performance Issues](#performance-issues)
- [Development Environment Issues](#development-environment-issues)

---

## Build Issues

### Issue: Build Fails with "Error MSB3073"

**Symptoms:**
- Compilation errors related to tool installation
- npm build fails
- Script execution fails

**Solutions:**

```bash
# Clear all caches
dotnet clean
dotnet nuget locals all --clear
npm cache clean --force

# Reinstall dependencies
dotnet restore
npm install

# Rebuild
dotnet build --no-incremental
```

### Issue: "tsc is not recognized"

**Symptoms:**
- Build fails with "tsc is not recognized as an internal or external command"
- TypeScript compiler not found

**Solutions:**

```bash
# Install TypeScript globally
npm install -g typescript

# Or install in project
npm install --save-dev typescript

# Navigate to web project and rebuild
cd src/Presentations/OnForkHub.Web
npm install
dotnet build
```

### Issue: NuGet Package Resolution Fails

**Symptoms:**
- "Unable to resolve reference to assembly" errors
- Package not found errors

**Solutions:**

```bash
# Check NuGet.config
cat NuGet.config

# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages explicitly
dotnet nuget push ... (if needed)
dotnet restore --no-cache

# Update packages
dotnet package update --interactive
```

### Issue: .NET Workload Warnings

**Symptoms:**
- "Updates of load work are available" messages
- Runtime compatibility warnings

**Solutions:**

```bash
# List available workloads
dotnet workload list

# Update workloads
dotnet workload update

# Install specific workload
dotnet workload install aspnet-runtime
```

---

## GitHub CLI Issues

### Issue: Authentication Fails

**Symptoms:**
- "Your credentials are invalid" error
- Unable to connect to GitHub
- `gh auth login` fails

**Solutions:**

```bash
# Reset authentication completely
gh auth logout
gh auth status

# Re-authenticate
gh auth login

# Select authentication method (HTTPS recommended)
# -> GitHub.com
# -> HTTPS
# -> Y (Authenticate Git with your GitHub credentials?)
# -> Paste authentication code from browser

# Verify new authentication
gh auth status
```

### Issue: Repository Not Found

**Symptoms:**
- "Repository not found" error when cloning
- Authentication succeeds but repo access fails

**Solutions:**

```bash
# Check if you have access to the repository
gh repo view RondineleG/OnForkHub

# Verify SSH keys if using SSH
gh ssh-key list

# Add SSH key if needed
gh ssh-key add ~/.ssh/id_rsa.pub

# Re-clone with correct method
gh repo clone RondineleG/OnForkHub
cd OnForkHub
```

### Issue: Pull Request Creation Fails

**Symptoms:**
- `dtn -p` command fails
- "Permission denied" when creating PR
- "No commits to create PR"

**Solutions:**

```bash
# Ensure working tree is clean
git status

# Commit all changes
git add .
git commit -m "feat: your changes"

# Verify current branch
git branch --show-current

# Check if branch is published
git flow feature publish $(git branch --show-current | cut -d/ -f2)

# Retry PR creation
dtn -p

# Manual PR creation as fallback
gh pr create --base dev --head feature/your-feature-name --title "Your PR Title"
```

---

## Git Flow Issues

### Issue: Git Flow Not Initialized

**Symptoms:**
- "Not a gitflow-enabled repo yet" error
- `git flow` commands don't work

**Solutions:**

```bash
# Initialize Git Flow
git flow init -d

# Or with custom configuration
git flow init -f -d

# Verify initialization
git config --local gitflow.initialized
# Should return: true

# Check configuration
git config --local --list | grep gitflow
```

### Issue: "Local branch 'master' does not exist"

**Symptoms:**
- Git Flow init fails
- Cannot initialize with default settings

**Solutions:**

```bash
# Configure production branch (using 'main' instead of 'master')
git config --local gitflow.branch.master main

# Configure development branch
git config --local gitflow.branch.develop dev

# Try initialization again
git flow init -f -d --defaults

# Verify
git branch -a
```

### Issue: Feature Branch Won't Merge

**Symptoms:**
- Merge conflicts when finishing feature
- `git flow feature finish` fails
- Uncommitted changes prevent finishing

**Solutions:**

```bash
# Check current status
git status

# Commit all changes
git add .
git commit -m "fix: resolve conflicts"

# Retry feature finish
git flow feature finish feature-name

# If still failing, manually resolve conflicts
git merge dev
# Resolve conflicts in editor
git add .
git commit -m "Merge conflicts resolved"
```

### Issue: Feature Branch Already Exists Remotely

**Symptoms:**
- "Branch already exists" when starting feature
- Previous feature branch not cleaned up

**Solutions:**

```bash
# List all branches
git branch -a

# Delete local branch
git branch -d feature/old-feature

# Delete remote branch
git push origin --delete feature/old-feature

# Clean up local tracking
git fetch --prune

# Start new feature
git flow feature start new-feature
```

---

## Docker Issues

### Issue: Docker Container Won't Start

**Symptoms:**
- Container exits immediately
- "Docker daemon not running"
- Port already in use

**Solutions:**

```bash
# Check Docker status
docker ps -a

# Start Docker daemon
# Windows: Start Docker Desktop application
# Mac: open /Applications/Docker.app
# Linux: sudo systemctl start docker

# Check if port is in use
netstat -ano | findstr :80
netstat -ano | findstr :9000

# Stop service using the port
taskkill /PID <PID> /F

# Rebuild container
docker build -t on-fork-hub -f .devcontainer/Dockerfile .
docker run --rm -it -v "$(pwd):/app" -w /app on-fork-hub bash
```

### Issue: Volume Mount Fails

**Symptoms:**
- "Cannot create volume"
- Permission denied errors
- Files not syncing between host and container

**Solutions:**

```bash
# Verify docker-compose.yml paths
cat docker-compose.yml

# Check volume permissions
ls -la /path/to/volume

# Use absolute paths in docker commands
docker run -v /absolute/path:/container/path ...

# On Windows, use full paths
docker run -v C:\Dev\OnForkHub:/app ...

# Restart Docker service
docker system prune
docker volume prune
```

---

## Database Issues

### Issue: RavenDB Connection Fails

**Symptoms:**
- "Cannot connect to RavenDB"
- Connection timeout errors
- 401 Unauthorized errors

**Solutions:**

```bash
# Verify RavenDB is running
docker ps | grep ravendb

# Check connection string in settings
cat appsettings.json | grep -i ravendb

# Test connection manually
curl http://localhost:8080

# Restart RavenDB container
docker restart ravendb

# Check RavenDB logs
docker logs ravendb
```

### Issue: CosmosDB Authentication Failed

**Symptoms:**
- "Unauthorized" errors
- Authentication failed
- Connection timeout

**Solutions:**

```bash
# Verify connection string
echo $COSMOS_CONNECTION_STRING

# Check account key
# Get from Azure Portal -> Azure Cosmos DB account -> Keys

# Verify network connectivity
ping cosmos.azure.com

# Test connection with Azure CLI
az cosmosdb database list --resource-group <rg-name> --account-name <account>

# Update connection string
# In appsettings.json or user secrets:
dotnet user-secrets set "CosmosDb:ConnectionString" "your-connection-string"
```

### Issue: SQL Server Connection Fails

**Symptoms:**
- "Cannot open database"
- Login failed for user
- Network error

**Solutions:**

```bash
# Verify SQL Server is running
docker ps | grep mssql

# Check SQL Server logs
docker logs sql-server

# Test connection
sqlcmd -S localhost -U sa -P YourPassword

# Verify connection string
cat appsettings.json | grep -i "Server="

# Restart container
docker restart sql-server

# Apply migrations
dotnet ef database update
```

### Issue: Database Migration Fails

**Symptoms:**
- Migration pending errors
- "There are pending model changes"
- Model conflicts

**Solutions:**

```bash
# List pending migrations
dotnet ef migrations list

# Remove last migration if new
dotnet ef migrations remove

# Create new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Force update (use with caution)
dotnet ef database update --context YourDbContext --force
```

---

## Testing Issues

### Issue: Tests Won't Run

**Symptoms:**
- "No test adapter found"
- Test discovery fails
- VSTest errors

**Solutions:**

```bash
# Verify test projects have correct framework
cat test/*/bin/Debug/*/OnForkHub*.*.dll

# Rebuild test projects
dotnet build test/Core/OnForkHub.Core.Test
dotnet build test/Persistence/OnForkHub.Persistence.Test

# Run tests explicitly
dotnet test --no-build

# Verbose output for debugging
dotnet test --no-build -v detailed
```

### Issue: Test Timeout

**Symptoms:**
- Tests hang or timeout
- Long running tests fail
- Integration tests never complete

**Solutions:**

```bash
# Increase test timeout
dotnet test --no-build /p:TestTimeout=60000

# Run single test for debugging
dotnet test --no-build --filter "ClassName=YourClass"

# Check for deadlocks
# Add breakpoint and use Visual Studio Debugger

# Run tests sequentially
dotnet test --no-build --parallel 0
```

### Issue: Mock/Stub Setup Fails

**Symptoms:**
- "Cannot create mock"
- Substitutes not working
- Setup exceptions

**Solutions:**

```bash
# Ensure interface exists
grep -r "interface IYourService" src/

# Verify NSubstitute package
cat *.csproj | grep -i nsubstitute

# Check mock setup syntax
# Example:
var mock = Substitute.For<IService>();
mock.GetData(Arg.Any<int>()).Returns("test");

# Use concrete implementations for simple cases
var implementation = new ConcreteService();
```

---

## Performance Issues

### Issue: Slow Build Times

**Symptoms:**
- Build takes > 1 minute
- Regular incremental builds are slow
- CI/CD pipeline timeouts

**Solutions:**

```bash
# Clean everything
dotnet clean
git clean -xfd

# Use faster build
dotnet build -c Release --no-restore

# Parallel build with more threads
dotnet build -m:8

# Disable analyzers temporarily
dotnet build -p:EnforceCodeStyleInBuild=false

# Profile build time
dotnet build /p:DetailedSummary=true

# Analyze slow projects
dotnet build /p:NoWarn=,/p:TreatWarningsAsErrors=false
```

### Issue: Application Runs Slowly

**Symptoms:**
- Page loads take > 5 seconds
- API responses slow
- High CPU/memory usage

**Solutions:**

```bash
# Profile application
dotnet run --configuration Release

# Check database query performance
# Enable SQL logging in appsettings.json
"Microsoft.EntityFrameworkCore": "Debug"

# Review slow queries
# Check indices and execution plans

# Optimize LINQ queries
// Bad
var items = dbContext.Items.ToList().Where(x => x.Active);
// Good
var items = dbContext.Items.Where(x => x.Active).ToList();

# Implement caching
services.AddDistributedMemoryCache();
services.AddScoped<ICacheService, MemoryCacheService>();
```

### Issue: High Memory Usage

**Symptoms:**
- Application uses > 500MB RAM
- Memory leak suspected
- Garbage collection issues

**Solutions:**

```bash
# Monitor memory
dotnet run --configuration Release | grep -i memory

# Enable GC logging
set DOTNET_GCLogLevel=verbose
set DOTNET_GCLogFileSize=67108864

# Use memory profiler
# Visual Studio -> Debug -> Performance Profiler -> Memory Usage

# Check for circular references
// Review large object collections
// Implement IDisposable for expensive resources
```

---

## Development Environment Issues

### Issue: VS Code Extensions Won't Load

**Symptoms:**
- C# extension not working
- IntelliSense broken
- Debugging unavailable

**Solutions:**

```bash
# Update VS Code
# Help -> Check for Updates

# Reinstall C# Dev Kit
# Extensions -> Search "C# Dev Kit" -> Uninstall -> Install

# Reload VS Code
Ctrl+Shift+P -> Developer: Reload Window

# Check extension logs
# Help -> Toggle Developer Tools -> Console tab
```

### Issue: Debugger Won't Attach

**Symptoms:**
- Breakpoints not hit
- "Unable to attach to process"
- Launch.json issues

**Solutions:**

```bash
# Verify launch.json configuration
cat .vscode/launch.json

# Kill any running dotnet processes
taskkill /IM dotnet.exe /F

# Rebuild in Debug mode
dotnet build --configuration Debug

# Start debugger
F5 or Debug -> Start Debugging

# Check if port 5000 is available
netstat -ano | findstr :5000
```

### Issue: npm Dependencies Won't Install

**Symptoms:**
- npm install fails
- Missing modules
- Version conflicts

**Solutions:**

```bash
# Clear npm cache
npm cache clean --force

# Remove node_modules and package-lock.json
rm -r node_modules
rm package-lock.json

# Reinstall
npm install

# For specific package issues
npm install package-name@latest

# Check for vulnerabilities
npm audit fix
npm audit fix --force
```

### Issue: Husky Git Hooks Fail

**Symptoms:**
- Commit blocked by pre-commit hook
- "husky command not found"
- Post-merge hook errors

**Solutions:**

```bash
# Reinstall Husky
dotnet husky uninstall
dotnet husky install

# Check hook permissions
ls -la .husky/

# Make hooks executable (Linux/Mac)
chmod +x .husky/*

# Reinstall dotnet tools
dotnet tool restore

# Skip hooks temporarily (use with caution)
git commit --no-verify -m "message"
```

---

## Getting Additional Help

If you encounter issues not covered here:

1. **Check existing GitHub Issues**: https://github.com/RondineleG/OnForkHub/issues
2. **Search documentation**: Refer to docs/ directory
3. **Check logs**: Review build.log and diag.log
4. **Ask on Telegram**: https://t.me/OnForkHub
5. **Email support**: rondineleg@gmail.com

---

## Contributing Solutions

Found a solution to an issue not listed here? Please contribute:

1. Fork the repository
2. Add solution to this document
3. Submit a pull request
4. Include example commands and outcomes

---

*Last Updated: 2025-11-07*
