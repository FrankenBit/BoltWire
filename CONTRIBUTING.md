# Contributing to BoltWire

Thank you for your interest in contributing to BoltWire! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How Can I Contribute?](#how-can-i-contribute)
- [Development Workflow](#development-workflow)
- [Branch Naming Conventions](#branch-naming-conventions)
- [Commit Message Guidelines](#commit-message-guidelines)
- [Pull Request Process](#pull-request-process)
- [Coding Style Guidelines](#coding-style-guidelines)
- [Testing Guidelines](#testing-guidelines)
- [Running Linters](#running-linters)
- [Filing Issues](#filing-issues)

## Code of Conduct

This project and everyone participating in it is governed by our [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## How Can I Contribute?

### Reporting Bugs

- Check the [issue tracker](https://github.com/FrankenBit/BoltWire/issues) to see if the bug has already been reported
- If not, create a new issue using the bug report template
- Provide detailed information including steps to reproduce, expected behavior, and actual behavior
- Include your environment details (Unity version, .NET version, OS)

### Suggesting Enhancements

- Open an issue with the label `enhancement`
- Clearly describe the enhancement and its use case
- Explain why this feature would be useful to most users

### Contributing Code

1. Fork the repository
2. Create a feature branch following our naming conventions
3. Make your changes following our coding style
4. Add or update tests as needed
5. Ensure all tests pass
6. Submit a pull request

## Development Workflow

### 1. Fork and Clone

```bash
# Fork the repository on GitHub, then clone your fork
git clone https://github.com/YOUR-USERNAME/BoltWire.git
cd BoltWire

# Add the upstream repository
git remote add upstream https://github.com/FrankenBit/BoltWire.git
```

### 2. Set Up Development Environment

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests to ensure everything works
dotnet test
```

### 3. Create a Feature Branch

```bash
# Update your main branch
git checkout main
git pull upstream main

# Create a new branch
git checkout -b feat/your-feature-name
```

### 4. Make Your Changes

- Write clean, readable code
- Follow the project's coding style
- Add tests for new functionality
- Update documentation as needed

### 5. Test Your Changes

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Check code formatting
dotnet format --verify-no-changes
```

### 6. Commit Your Changes

Follow our commit message guidelines (see below).

### 7. Push and Create Pull Request

```bash
# Push your changes to your fork
git push origin feat/your-feature-name

# Create a pull request on GitHub
```

## Branch Naming Conventions

Use the following prefixes for your branches:

- `feat/` - New features
  - Example: `feat/add-lazy-loading`
- `fix/` - Bug fixes
  - Example: `fix/null-reference-in-scope`
- `docs/` - Documentation changes
  - Example: `docs/update-readme-examples`
- `refactor/` - Code refactoring
  - Example: `refactor/simplify-registration`
- `test/` - Test-related changes
  - Example: `test/add-scope-tests`
- `chore/` - Maintenance tasks
  - Example: `chore/update-dependencies`

## Commit Message Guidelines

We follow the [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, missing semi-colons, etc.)
- `refactor`: Code changes that neither fix a bug nor add a feature
- `perf`: Performance improvements
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools

### Examples

```
feat(registry): add support for named service registrations

Added the ability to register multiple implementations of the same
interface with different names, allowing resolution by name.

Closes #123
```

```
fix(scope): prevent memory leak in nested scopes

Fixed an issue where nested scopes weren't properly disposing of
transient services, leading to memory leaks in long-running games.

Fixes #456
```

## Pull Request Process

### Before Submitting

Ensure your PR meets the following requirements:

- [ ] Code follows the project's coding style guidelines
- [ ] All tests pass (`dotnet test`)
- [ ] Code is properly formatted (`dotnet format --verify-no-changes`)
- [ ] New features include appropriate tests
- [ ] Documentation is updated (README, XML comments, etc.)
- [ ] Commit messages follow the conventional commits format
- [ ] No merge conflicts with the target branch
- [ ] PR description clearly explains the changes

### PR Template Checklist

When creating a PR, include:

1. **Description**: What does this PR do?
2. **Motivation**: Why is this change needed?
3. **Testing**: How was this tested?
4. **Screenshots**: If applicable (UI changes, console output)
5. **Breaking Changes**: Any breaking changes?
6. **Related Issues**: Link to related issues

### Review Process

1. At least one maintainer review is required
2. All CI checks must pass
3. Address review comments promptly
4. Once approved, a maintainer will merge your PR

## Coding Style Guidelines

### General C# Guidelines

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful, descriptive names for variables, methods, and classes
- Keep methods small and focused on a single responsibility
- Prefer composition over inheritance

### Nullable Reference Types

This project has nullable reference types enabled project-wide:

```csharp
// Good: Explicitly handle nullability
public void ProcessService(IService? service)
{
    if (service is null)
    {
        throw new ArgumentNullException(nameof(service));
    }
    
    service.Execute();
}

// Bad: Ignoring null possibility
public void ProcessService(IService service)
{
    service.Execute(); // May throw if service is null
}
```

### Code Formatting

Use `dotnet format` to ensure consistent formatting:

```bash
# Format all code
dotnet format

# Check formatting without changes
dotnet format --verify-no-changes
```

### EditorConfig

The project includes an `.editorconfig` file. Ensure your IDE respects these settings:

- Indent style: spaces
- Indent size: 4
- End of line: LF
- Charset: UTF-8
- Insert final newline: true
- Trim trailing whitespace: true

### XML Documentation Comments

Public APIs should include XML documentation:

```csharp
/// <summary>
/// Registers a singleton service in the container.
/// </summary>
/// <typeparam name="TService">The service type to register.</typeparam>
/// <typeparam name="TImplementation">The implementation type.</typeparam>
/// <returns>The service collection for chaining.</returns>
public IServiceCollection AddSingleton<TService, TImplementation>()
    where TService : class
    where TImplementation : class, TService
{
    // Implementation
}
```

### File Organization

- One class per file (except nested classes)
- File name should match the class name
- Organize using statements alphabetically
- Place System namespaces first

### Naming Conventions

- `PascalCase` for class names, method names, properties
- `camelCase` for local variables, parameters
- `_camelCase` for private fields
- `IPascalCase` for interfaces (prefix with 'I')
- `UPPER_CASE` for constants (when appropriate)

## Testing Guidelines

### Test Framework

This project uses NUnit for testing. Follow these guidelines:

### Test Structure

Use the Arrange-Act-Assert (AAA) pattern:

```csharp
[Test]
public void AddSingleton_WithValidTypes_RegistersService()
{
    // Arrange
    var services = new ServiceCollection();
    
    // Act
    services.AddSingleton<IService, ServiceImplementation>();
    var provider = services.BuildServiceProvider();
    
    // Assert
    var service = provider.GetService<IService>();
    Assert.That(service, Is.Not.Null);
    Assert.That(service, Is.InstanceOf<ServiceImplementation>());
}
```

### Test Naming

Use descriptive test names that explain the scenario:

```
MethodName_Scenario_ExpectedBehavior
```

Examples:
- `GetService_WithUnregisteredType_ReturnsNull`
- `CreateScope_WithDisposedProvider_ThrowsObjectDisposedException`
- `BuildServiceProvider_WithCircularDependency_ThrowsInvalidOperationException`

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test
dotnet test --filter "FullyQualifiedName~ServiceProviderTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run tests in watch mode
dotnet watch test
```

### Test Coverage

- Aim for at least 80% code coverage for new features
- Focus on testing public APIs and critical paths
- Include edge cases and error conditions
- Test both success and failure scenarios

### Mock Objects

Use appropriate mocking when needed:

```csharp
[Test]
public void Constructor_WithNullDependency_ThrowsArgumentNullException()
{
    // Arrange & Act & Assert
    Assert.Throws<ArgumentNullException>(() => 
        new ServiceProvider(null!));
}
```

## Running Linters

### Code Formatting

```bash
# Check code formatting
dotnet format --verify-no-changes

# Auto-fix formatting issues
dotnet format
```

### Code Analysis

The project uses Roslyn analyzers enabled in Debug configuration:

```bash
# Build with code analysis
dotnet build -c Debug

# View warnings and errors
dotnet build -c Debug -v detailed
```

### Common Analyzers

The project may include:
- StyleCop.Analyzers (code style)
- Microsoft.CodeAnalysis.NetAnalyzers (code quality)
- SonarAnalyzer.CSharp (code quality and security)

### Suppressing Warnings

Only suppress warnings when absolutely necessary, with justification:

```csharp
#pragma warning disable CA1062 // Validate arguments of public methods
public void Process(object input)
{
    // Justification: Input validation handled by caller
    input.ToString();
}
#pragma warning restore CA1062
```

## Filing Issues

### Bug Reports

Use the bug report template (`.github/ISSUE_TEMPLATE/bug_report.md`) and include:

1. **Description**: Clear description of the bug
2. **Steps to Reproduce**: Detailed steps to reproduce the issue
3. **Expected Behavior**: What you expected to happen
4. **Actual Behavior**: What actually happened
5. **Environment**:
   - Unity version
   - .NET SDK version
   - Operating System
   - BoltWire version
6. **Additional Context**: Screenshots, logs, code samples

### Feature Requests

When requesting a feature:

1. **Problem**: Describe the problem you're trying to solve
2. **Proposed Solution**: Your suggested approach
3. **Alternatives**: Other approaches you've considered
4. **Use Case**: Real-world scenario where this would be useful

### Questions

For questions:
- Check existing documentation and Wiki first
- Use GitHub Discussions for general questions
- Use Issues only for actionable items

## Development Tips

### Setting Up IDE

**Visual Studio 2022:**
- Install "Unity Game Development" workload
- Enable code analysis in project properties

**Rider:**
- Install Unity support plugin
- Enable code inspections

**VS Code:**
- Install C# extension
- Install Unity extension

### Useful Commands

```bash
# Clean build artifacts
dotnet clean

# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Create NuGet package
dotnet pack

# Format code
dotnet format
```

### Debugging

For debugging Unity-specific code:
1. Attach Unity debugger to the Unity Editor
2. Set breakpoints in your IDE
3. Use Unity's Debug.Log for runtime logging

For non-Unity tests:
```bash
# Run tests in debug mode
dotnet test --logger "console;verbosity=detailed"
```

## Getting Help

If you need help:

1. Check the [Wiki](https://github.com/FrankenBit/BoltWire/wiki)
2. Search [existing issues](https://github.com/FrankenBit/BoltWire/issues)
3. Ask in [GitHub Discussions](https://github.com/FrankenBit/BoltWire/discussions)
4. Reach out to maintainers at contact@frankenbit.de

## Recognition

Contributors will be recognized in:
- CHANGELOG.md for significant contributions
- GitHub contributor graph
- Project acknowledgments

Thank you for contributing to BoltWire! 🎮⚡
