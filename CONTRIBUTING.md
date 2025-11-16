# Contributing to BoltWire

Thank you for your interest in contributing to BoltWire! This document provides guidelines and instructions for contributing to the project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Guidelines](#issue-guidelines)

## Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

## How to Contribute

### Reporting Bugs

1. Check if the bug has already been reported in [Issues](https://github.com/swalex/BoltWire/issues)
2. If not, create a new issue using the bug report template
3. Provide as much detail as possible:
   - Unity version and .NET version
   - Operating system
   - Steps to reproduce
   - Expected vs actual behavior
   - Sample code or project (if applicable)
   - Stack traces or error logs

### Suggesting Features

1. Check [Issues](https://github.com/swalex/BoltWire/issues) to see if the feature has been suggested
2. Create a new issue with the "enhancement" label
3. Clearly describe:
   - The problem you're trying to solve
   - Your proposed solution
   - Alternative solutions you've considered
   - Any implementation ideas

### Submitting Changes

1. Fork the repository
2. Create a new branch following our naming conventions
3. Make your changes
4. Write or update tests
5. Ensure all tests pass
6. Update documentation
7. Submit a pull request

## Development Workflow

### 1. Fork and Clone

```bash
# Fork the repository on GitHub, then:
git clone https://github.com/YOUR_USERNAME/BoltWire.git
cd BoltWire
git remote add upstream https://github.com/swalex/BoltWire.git
```

### 2. Create a Branch

Use descriptive branch names following these conventions:

- `feat/feature-name` - New features
- `fix/bug-description` - Bug fixes
- `docs/description` - Documentation updates
- `test/description` - Test additions or modifications
- `refactor/description` - Code refactoring
- `chore/description` - Maintenance tasks

Examples:
```bash
git checkout -b feat/add-lazy-injection
git checkout -b fix/null-reference-in-scope
git checkout -b docs/update-quickstart-guide
```

### 3. Make Your Changes

- Write clean, readable code
- Follow the coding standards (see below)
- Add or update tests as needed
- Update documentation for new features or API changes
- Keep commits focused and atomic

### 4. Commit Your Changes

We follow [Conventional Commits](https://www.conventionalcommits.org/) specification:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Code style changes (formatting, no logic change)
- `refactor`: Code changes that neither fix bugs nor add features
- `test`: Adding or updating tests
- `chore`: Maintenance tasks, tooling, dependencies

**Examples:**
```bash
git commit -m "feat: add support for lazy service resolution"
git commit -m "fix: resolve null reference when disposing empty scope"
git commit -m "docs: update installation instructions for Unity 6.0"
git commit -m "test: add tests for collection injection"
git commit -m "refactor: simplify service registry implementation"
```

For breaking changes:
```bash
git commit -m "feat!: change IServiceProvider interface signature

BREAKING CHANGE: GetService now requires a non-null return type parameter"
```

### 5. Keep Your Fork Updated

```bash
git fetch upstream
git rebase upstream/main
```

### 6. Push and Create a Pull Request

```bash
git push origin your-branch-name
```

Then create a pull request on GitHub.

## Coding Standards

### C# Style Guidelines

#### General Principles

- Follow [Microsoft's C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful, descriptive names
- Keep methods small and focused
- Prefer composition over inheritance
- Write self-documenting code; use comments sparingly and only when necessary

#### Formatting

Use `dotnet format` to automatically format your code:

```bash
dotnet format
```

The project includes an `.editorconfig` file that defines formatting rules. Ensure your IDE is configured to respect it.

**Key formatting rules:**
- Indentation: 4 spaces (no tabs)
- Line length: Prefer 120 characters, hard limit at 160
- Braces: Opening brace on new line (Allman style)
- Use `var` for obvious types
- One statement per line

#### Nullable Reference Types

BoltWire uses nullable reference types. Always use them correctly:

```csharp
#nullable enable

// Good
public string? GetOptionalValue() { ... }
public string GetRequiredValue() { ... }

// Bad - missing nullable annotation
public string GetOptionalValue() { return null; } // Warning!
```

#### Naming Conventions

- **PascalCase**: Classes, interfaces, methods, properties, public fields
  ```csharp
  public class ServiceProvider { }
  public interface IServiceRegistry { }
  public void RegisterService() { }
  public string ServiceName { get; set; }
  ```

- **camelCase**: Private fields (with `_` prefix), local variables, parameters
  ```csharp
  private readonly IServiceProvider _serviceProvider;
  public void DoWork(int itemCount) { }
  ```

- **Interfaces**: Prefix with `I`
  ```csharp
  public interface IServiceProvider { }
  ```

- **Type parameters**: Prefix with `T`
  ```csharp
  public class ServiceRegistry<TService, TImplementation> { }
  ```

#### Unity-Specific Guidelines

**MonoBehaviour Conventions:**
```csharp
// Use Unity's message method naming (PascalCase)
private void Awake() { }
private void Start() { }
private void Update() { }
private void OnDestroy() { }

// Serialize fields for inspector editing
[SerializeField] private float _moveSpeed = 5f;
```

**Assembly Definition Files (asmdef):**
- Keep runtime code in the `Runtime` assembly
- Place editor-only code in `Editor` folders with separate asmdef files
- Never reference editor assemblies from runtime code
- Use asmdef references, not DLL references
- Name asmdef files: `<Company>.<Product>.<Assembly>.asmdef`

**Avoid Editor-Only Code in Runtime:**
```csharp
// Bad - Editor code in runtime assembly
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MyRuntimeClass
{
    #if UNITY_EDITOR
    public void EditorOnlyMethod() { } // Don't do this in runtime assemblies
    #endif
}

// Good - Separate editor assembly
// Runtime/MyRuntimeClass.cs
public class MyRuntimeClass { }

// Editor/MyRuntimeClassEditor.cs (in separate assembly)
using UnityEditor;
public class MyRuntimeClassEditor : Editor { }
```

### Code Analysis

Enable and address Roslyn analyzer warnings:

```bash
# Run Roslyn analyzers
dotnet build /p:TreatWarningsAsErrors=true

# Fix formatting issues
dotnet format
```

Common analyzers to watch:
- **CA1031**: Do not catch general exception types
- **CA1062**: Validate arguments of public methods
- **CA1508**: Avoid dead conditional code
- **CA2000**: Dispose objects before losing scope

## Testing Guidelines

### Writing Tests

BoltWire uses **NUnit** for testing. Tests should be:
- **Focused**: Test one thing per test method
- **Isolated**: Tests should not depend on each other
- **Fast**: Keep tests quick to encourage frequent running
- **Reliable**: Tests should produce consistent results

### Unity Test Runner

BoltWire supports both Edit Mode and Play Mode tests:

#### Edit Mode Tests

Fast tests that run in the editor without entering Play mode:

```csharp
using NUnit.Framework;
using FrankenBit.BoltWire;

namespace FrankenBit.BoltWire.Tests
{
    [TestFixture]
    public class ServiceCollectionTests
    {
        [Test]
        public void AddSingleton_WithImplementation_RegistersSuccessfully()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Act
            services.AddSingleton<ITestService, TestService>();
            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ITestService>();
            
            // Assert
            Assert.IsNotNull(service);
            Assert.IsInstanceOf<TestService>(service);
        }
        
        [Test]
        public void AddSingleton_ReturnsSameInstance()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddSingleton<ITestService, TestService>();
            var provider = services.BuildServiceProvider();
            
            // Act
            var service1 = provider.GetService<ITestService>();
            var service2 = provider.GetService<ITestService>();
            
            // Assert
            Assert.AreSame(service1, service2);
        }
    }
}
```

#### Play Mode Tests

Tests that run in Play mode, useful for testing Unity-specific features:

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

namespace FrankenBit.BoltWire.Tests
{
    [TestFixture]
    public class UnityIntegrationTests
    {
        [UnityTest]
        public IEnumerator ServiceProvider_WorksWithMonoBehaviour()
        {
            // Arrange
            var go = new GameObject();
            var installer = go.AddComponent<TestInstaller>();
            
            // Act
            yield return null; // Wait one frame for Awake/Start
            
            // Assert
            Assert.IsNotNull(installer.ServiceProvider);
            
            // Cleanup
            Object.Destroy(go);
        }
    }
}
```

### Running Tests

#### In Unity

1. Open Test Runner: `Window > General > Test Runner`
2. Select `EditMode` or `PlayMode` tab
3. Click `Run All` or right-click specific tests to run

#### Using .NET CLI

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test project
dotnet test Tests/Editor/FrankenBit.BoltWire.Tests.Editor.csproj

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Coverage

Aim for high test coverage, especially for:
- Public APIs
- Edge cases and error conditions
- Different service lifetimes
- Scope creation and disposal

### Testing Best Practices

- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Use Arrange-Act-Assert pattern
- Test both success and failure paths
- Clean up resources in tests (use `[TearDown]` if needed)
- Avoid testing implementation details; focus on behavior
- Use `[TestCase]` for parameterized tests

```csharp
[TestCase(ServiceLifetime.Singleton)]
[TestCase(ServiceLifetime.Scoped)]
[TestCase(ServiceLifetime.Transient)]
public void AddService_WithDifferentLifetimes_RegistersCorrectly(ServiceLifetime lifetime)
{
    // Test implementation
}
```

## Pull Request Process

### Before Submitting

**Pre-submission checklist:**

- [ ] Code follows the style guidelines
- [ ] All tests pass (both Unity Test Runner and `dotnet test`)
- [ ] New tests added for new functionality
- [ ] Documentation updated (README, XML comments, etc.)
- [ ] No compiler warnings
- [ ] Code formatted with `dotnet format`
- [ ] Commit messages follow Conventional Commits
- [ ] Branch is up-to-date with `main`

### PR Checklist

When creating your pull request, include the following:

1. **Clear Title**: Use format `<type>: <description>`
   - Example: `feat: add lazy service resolution support`

2. **Description**: Explain what and why
   - What changes were made
   - Why these changes were necessary
   - How to test the changes
   - Link to related issues

3. **Testing Instructions**: How reviewers can test your changes
   - Step-by-step instructions
   - Expected results
   - Any special setup needed

4. **Screenshots/GIFs**: If applicable, for UI or visual changes

### Example PR Description

```markdown
## Summary
Adds support for lazy service resolution using `Lazy<T>` wrapper.

## Motivation
Resolves #123. Users need a way to defer service instantiation until first use, particularly for expensive objects.

## Changes
- Added `LazyServiceResolver<T>` class
- Updated `ServiceProvider` to recognize `Lazy<T>` requests
- Added extension method `AddLazy<T>()` to `IServiceCollection`
- Added comprehensive tests for lazy resolution

## Testing
1. Open Test Runner in Unity
2. Run `LazyServiceResolverTests`
3. All tests should pass
4. Alternatively, run `dotnet test` from command line

## Checklist
- [x] Tests added/updated
- [x] Documentation updated
- [x] Code formatted with `dotnet format`
- [x] All tests pass
- [x] No compiler warnings
- [x] Commit messages follow Conventional Commits
```

### Review Process

- Maintainers will review your PR
- Address review comments and feedback
- Push additional commits to your branch
- Once approved, a maintainer will merge your PR

### After Merge

- Delete your branch
- Update your fork:
  ```bash
  git checkout main
  git pull upstream main
  git push origin main
  ```

## Issue Guidelines

### Creating Issues

Use the appropriate issue template:
- **Bug Report**: For reporting bugs
- **Feature Request**: For suggesting new features
- **Question**: For asking questions

### Issue Labels

Issues are labeled to help with organization:
- `bug` - Confirmed bugs
- `enhancement` - New features or improvements
- `documentation` - Documentation improvements
- `good first issue` - Good for newcomers
- `help wanted` - Extra attention needed
- `question` - Questions about usage
- `wontfix` - Won't be addressed
- `duplicate` - Duplicate of another issue
- `invalid` - Invalid or off-topic

### Issue Lifecycle

1. **Open**: Issue is created
2. **Triaged**: Maintainers review and label
3. **In Progress**: Someone is working on it
4. **Review**: PR is under review
5. **Closed**: Issue is resolved or won't be fixed

## Questions?

If you have questions about contributing, feel free to:
- Open an issue with the "question" label
- Start a discussion in [GitHub Discussions](https://github.com/swalex/BoltWire/discussions)

Thank you for contributing to BoltWire! 🎮⚡
