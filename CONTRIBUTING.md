# Contributing to OnForkHub

Thank you for considering contributing to OnForkHub! We welcome contributions from the community. This guide will help you get started.

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Commit Standards](#commit-standards)
- [Pull Request Process](#pull-request-process)
- [Code Style Guide](#code-style-guide)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)
- [Reporting Issues](#reporting-issues)

---

## Code of Conduct

We are committed to providing a welcoming and inclusive environment. Please be respectful and constructive in all interactions. Any form of harassment or discrimination will not be tolerated.

---

## Getting Started

### Prerequisites

- .NET 9 SDK
- Git
- GitHub CLI (gh)
- Node.js
- Docker (optional)

### First-Time Setup

```bash
# 1. Fork the repository on GitHub
# 2. Clone your fork
gh repo clone YOUR_USERNAME/OnForkHub
cd OnForkHub

# 3. Add upstream remote
git remote add upstream https://github.com/RondineleG/OnForkHub.git

# 4. Setup development environment
dotnet build
dotnet husky install
dotnet dev-certs https --trust

# 5. Verify setup
dotnet test --no-build
```

---

## Development Workflow

### 1. Sync with Main Repository

```bash
# Fetch latest changes from upstream
git fetch upstream

# Update your local main branch
git checkout main
git rebase upstream/main

# Update your local dev branch
git checkout dev
git rebase upstream/dev
```

### 2. Create Feature Branch

We use Git Flow with kebab-case naming convention:

```bash
# Feature branch
git flow feature start add-user-authentication
# Creates: feature/add-user-authentication

# Bugfix branch
git flow bugfix start fix-login-redirect
# Creates: bugfix/fix-login-redirect

# Hotfix branch (from main)
git flow hotfix start fix-critical-security
# Creates: hotfix/fix-critical-security
```

**Branch Naming Rules:**
- Use kebab-case (lowercase with hyphens)
- Be descriptive and concise
- Include issue number if applicable: `feature/add-user-auth-#123`

### 3. Make Changes

```bash
# Make your changes following coding standards
# Keep commits atomic and logical

# Stage changes
git add .

# Commit with proper message (see Commit Standards)
git commit -m "feat(auth): implement JWT authentication"

# Push to your fork
git push origin feature/add-user-authentication
```

### 4. Complete Feature

```bash
# Finish feature (creates PR automatically)
git flow feature finish add-user-authentication

# Or manually create PR
dtn -p  # Using CLI tool
# or
gh pr create --base dev --head feature/add-user-authentication
```

---

## Commit Standards

We follow [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Format

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Type

- `feat` – New feature
- `fix` – Bug fix
- `refactor` – Code refactoring (no behavior change)
- `perf` – Performance improvement
- `style` – Formatting/style changes (no logic change)
- `test` – Adding or updating tests
- `docs` – Documentation changes
- `chore` – Build, dependencies, or tooling changes
- `ci` – CI/CD pipeline changes
- `revert` – Revert previous commit

### Scope

Specify the area affected:
- `auth` – Authentication related
- `api` – API endpoints
- `core` – Core functionality
- `ui` – User interface
- `db` – Database related
- `infra` – Infrastructure
- `test` – Testing infrastructure

### Description

- Use imperative mood ("add feature" not "added feature")
- Don't capitalize first letter
- No period at the end
- Keep under 50 characters when possible

### Body

- Explain **what** and **why**, not **how**
- Wrap at 72 characters
- Separate from description with blank line

### Footer

- Reference issues: `Closes #123` or `Fixes #456`
- Reference breaking changes: `BREAKING CHANGE: description`

### Examples

#### Good Commits

```
feat(auth): implement multi-factor authentication

- Add SMS verification support
- Implement authenticator app integration
- Add backup codes generation

Closes #456
```

```
fix(api): resolve null pointer exception in category service

The service failed to handle null category names correctly.
Added proper null checks and error handling.

Fixes #789
```

```
refactor(core): simplify validation logic

Replaced nested if statements with early returns for better
readability and reduced cyclomatic complexity.
```

```
test(persistence): add comprehensive repository tests

Added tests for:
- CRUD operations
- Pagination
- Search functionality
- Error handling
```

```
docs(readme): add troubleshooting section

Added comprehensive troubleshooting guide covering:
- Common build issues
- Database connection problems
- Testing issues
```

#### Poor Commits (Avoid)

```
Fixed stuff               # Too vague
WIP: work in progress     # Incomplete
Updated files            # No context
URGENT!!!                # Excessive punctuation
```

---

## Pull Request Process

### Before Submitting

1. **Sync with upstream**
   ```bash
   git fetch upstream dev
   git rebase upstream/dev
   ```

2. **Run tests locally**
   ```bash
   dotnet clean
   dotnet build
   dotnet test --no-build
   ```

3. **Check code style**
   ```bash
   # StyleCop and analyzers run automatically during build
   dotnet build /p:EnforceCodeStyleInBuild=true
   ```

4. **Update documentation**
   - Add/update XML documentation for public APIs
   - Update README if adding new features
   - Update CHANGELOG.md

### PR Title

Follow commit message format:
```
feat(auth): add token refresh mechanism
fix(api): resolve timeout on large uploads
docs: expand API reference guide
```

### PR Description

```markdown
## Description
Brief description of changes

## Related Issue
Closes #123

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation

## How to Test
Steps to verify the changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] No breaking changes (or documented)
```

### Review Process

1. Maintain conversations positively and respectfully
2. Respond to reviewer feedback within 48 hours
3. Push changes to the same branch (don't create new PRs)
4. Mark conversations as resolved once addressed
5. Request re-review after making changes

### Approval & Merge

- Minimum 1 approval required
- All checks must pass
- Branch must be up-to-date with base branch
- Squash commits for cleaner history (recommended)

---

## Code Style Guide

### Naming Conventions

```csharp
// Classes: PascalCase
public class UserService { }

// Methods: PascalCase
public void CalculateTotal() { }

// Variables: camelCase
private string userName;
private int itemCount;

// Constants: UPPER_CASE_SNAKE
private const int MAX_RETRY_ATTEMPTS = 3;

// Properties: PascalCase
public string Name { get; set; }

// Interfaces: IPascalCase
public interface IUserService { }

// Generic types: use T prefix
public class Repository<TEntity> { }
```

### File Organization

```csharp
// 1. Using statements
using System;
using Microsoft.AspNetCore.Mvc;

// 2. Namespace
namespace OnForkHub.Application.Services
{
    // 3. Class declaration
    public class UserService
    {
        // 4. Private fields
        private readonly IUserRepository _repository;
        
        // 5. Constructors
        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }
        
        // 6. Public properties
        public string Name { get; set; }
        
        // 7. Public methods
        public void CreateUser() { }
        
        // 8. Private methods
        private bool ValidateUser() { }
    }
}
```

### Method Guidelines

- Max 15 lines per method
- Single responsibility principle
- Max 3 parameters (use objects for more)
- Avoid nested ternary operators
- Handle exceptions specifically

### Documentation

```csharp
/// <summary>
/// Calculates the total price with discounts applied.
/// </summary>
/// <param name="order">The order to calculate.</param>
/// <param name="discountPercentage">Discount percentage (0-100).</param>
/// <returns>Total price after discount.</returns>
/// <exception cref="ArgumentNullException">Thrown when order is null.</exception>
public decimal CalculateTotal(Order order, decimal discountPercentage)
{
    if (order == null)
        throw new ArgumentNullException(nameof(order));
        
    return order.Total * (1 - discountPercentage / 100);
}
```

---

## Testing Guidelines

### Test Structure

```csharp
[TestClass]
public class UserServiceTests
{
    private IUserRepository _userRepositoryMock;
    private UserService _userService;
    
    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userService = new UserService(_userRepositoryMock);
    }
    
    [TestMethod]
    [DisplayName("Should create user successfully")]
    public async Task CreateUser_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var user = new User { Name = "John Doe" };
        _userRepositoryMock.AddAsync(user).Returns(true);
        
        // Act
        var result = await _userService.CreateUserAsync(user);
        
        // Assert
        result.Should().BeTrue();
        await _userRepositoryMock.Received(1).AddAsync(user);
    }
}
```

### Test Coverage

- Aim for 80% code coverage
- Test happy path, edge cases, and error cases
- Use meaningful test names
- Keep tests isolated and independent
- Use mocks for external dependencies

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=UserServiceTests"

# Run with coverage
dotnet test /p:CollectCoverage=true

# Verbose output
dotnet test --verbosity detailed
```

---

## Documentation

### Markdown Files

- Use clear, concise language
- Include code examples
- Add table of contents for long documents
- Keep line length under 100 characters
- Use headers for organization

### Code Comments

- Write self-documenting code first
- Use comments for **why**, not **what**
- Keep comments up-to-date with code
- Avoid redundant comments

```csharp
// Bad
// Increment i
i++;

// Good
// Use exponential backoff for retry attempts
await Task.Delay((int)Math.Pow(2, retryCount) * 1000);
```

### README

- Clear project description
- Quick start guide
- Links to documentation
- Contributing guidelines
- License information

---

## Reporting Issues

### Bug Reports

Include:
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment (OS, .NET version, etc.)
- Screenshots/logs if applicable

### Feature Requests

Include:
- Clear description of feature
- Use case/motivation
- Proposed implementation (if any)
- Alternative solutions considered

### Discussion/Questions

Use Discussions tab for:
- How-to questions
- Design discussions
- Best practice questions

---

## Useful Commands

```bash
# View git config
git config --local --list

# Squash last N commits
git rebase -i HEAD~3

# Force push (use carefully)
git push -f origin feature-branch

# View commit log nicely
git log --oneline --graph --all

# Stash changes temporarily
git stash
git stash pop

# Reset to remote state
git reset --hard origin/dev
```

---

## Getting Help

- **Documentation:** Check `docs/` directory
- **Discussions:** GitHub Discussions tab
- **Issues:** Search existing issues
- **Telegram:** https://t.me/OnForkHub
- **Email:** rondineleg@gmail.com

---

## Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Project README

Thank you for contributing to OnForkHub! 🎉

---

*Last Updated: 2025-11-07*
