# BoltWire

> Another scary dependency injection framework for Unity 3D

BoltWire is a lightweight, fast dependency injection (DI) container designed specifically for Unity 3D projects. It provides a flexible service registration and resolution system with support for different service lifetimes, constructor injection, and Unity-specific integrations.

## About

BoltWire brings powerful dependency injection capabilities to Unity 3D while maintaining simplicity and performance. Built on .NET Standard 2.1, it seamlessly integrates with Unity's component model and supports modern C# features including nullable reference types.

**Key Highlights:**
- 🚀 Fast and lightweight DI container
- 🎮 Unity 3D integration (MonoBehaviour lifecycle support)
- 🔧 Multiple service lifetimes (Singleton, Scoped, Transient)
- 💉 Constructor injection and factory patterns
- 🧩 Collection-capable service registry
- ✅ Nullable reference types enabled
- 🎯 .NET Standard 2.1 compatible

## Features

- **Flexible Service Registration**: Register services as singletons, scoped, or transient instances
- **Constructor Selection**: Automatic greedy constructor selection for dependency resolution
- **Service Collections**: Support for registering and resolving multiple implementations
- **Factory Registration**: Register services using factory methods for complex initialization
- **Scope Management**: Create child scopes with isolated service instances
- **Startable Services**: Automatic initialization of services implementing `IStartable`
- **Composite Disposal**: Automatic cleanup of disposable dependencies
- **Unity Integration**: Special support for Unity MonoBehaviour components

## Getting Started

### Prerequisites

- **Unity**: 6.0.21f1 or later
- **.NET SDK**: .NET 8.0 SDK (for development and testing)
- **IDE**: Visual Studio 2022, Rider, or VS Code with C# extension

### Installation

This package is designed to be installed as a Unity Package Manager (UPM) package.

#### Option 1: Add via Git URL (Unity Package Manager)
1. Open Unity Package Manager (Window → Package Manager)
2. Click the "+" button and select "Add package from git URL"
3. Enter: `https://github.com/FrankenBit/BoltWire.git`

#### Option 2: Add to manifest.json
Add the following to your `Packages/manifest.json`:
```json
{
  "dependencies": {
    "de.frankenbit.boltwire": "https://github.com/FrankenBit/BoltWire.git"
  }
}
```

#### Option 3: Clone for Development
```bash
# Clone the repository
git clone https://github.com/FrankenBit/BoltWire.git
cd BoltWire

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

### Quickstart

Here's a simple example of using BoltWire in your Unity project:

```csharp
using FrankenBit.BoltWire;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private IServiceProvider _serviceProvider;

    void Awake()
    {
        // Create a service collection
        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<IGameManager, GameManager>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddTransient<IEnemyFactory, EnemyFactory>();
        
        // Build the service provider
        _serviceProvider = services.BuildServiceProvider();
        
        // Resolve and use services
        var gameManager = _serviceProvider.GetRequiredService<IGameManager>();
        gameManager.Initialize();
    }

    void OnDestroy()
    {
        // Clean up
        (_serviceProvider as IDisposable)?.Dispose();
    }
}
```

## Configuration

### Unity Package Configuration

The package is configured via `package.json` in the root directory:

```json
{
  "name": "de.frankenbit.boltwire",
  "version": "1.0.0-pre.1",
  "displayName": "BoltWire",
  "description": "Another scary dependency injection framework for Unity 3D.",
  "unity": "6.0",
  "dependencies": {}
}
```

### Project Build Configuration

The project uses MSBuild with `Directory.Build.props` for shared configuration:

```xml
<Project>
  <PropertyGroup>
    <LangVersion>preview</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

### Environment Variables

For development builds, you may need to configure Unity paths:
- **Windows**: Unity installation path is typically auto-detected
- **Linux/macOS**: Unity-specific code is excluded via conditional compilation

## Usage

### Basic Service Registration

```csharp
var services = new ServiceCollection();

// Singleton - single instance for the entire application
services.AddSingleton<ILogger, ConsoleLogger>();

// Scoped - one instance per scope
services.AddScoped<IGameSession, GameSession>();

// Transient - new instance every time
services.AddTransient<ICommand, MoveCommand>();
```

### Using Service Scopes

```csharp
using (var scope = serviceProvider.CreateScope())
{
    var scopedService = scope.ServiceProvider.GetRequiredService<IGameSession>();
    // Use the service within this scope
} // Scope and its services are disposed here
```

### Factory Registrations

```csharp
services.AddSingleton<IEnemyFactory>(provider => 
{
    var config = provider.GetRequiredService<IGameConfig>();
    return new EnemyFactory(config.EnemySettings);
});
```

### Running the Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

## Development

### Branching Strategy

We follow a standard Git workflow:
- `main` - Stable release branch
- `develop` - Integration branch for features
- `feat/feature-name` - New features
- `fix/bug-description` - Bug fixes
- `docs/description` - Documentation updates

### Coding Style

This project follows C# coding conventions with:
- **Nullable Reference Types**: Enabled project-wide
- **Language Version**: C# 10.0 (preview features for development)
- **Code Formatting**: Use `dotnet format` for consistent formatting
- **EditorConfig**: Configuration included in the repository

To format code:
```bash
# Format all code in the solution
dotnet format

# Check formatting without making changes
dotnet format --verify-no-changes
```

### Running Locally

```bash
# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Watch mode for continuous building
dotnet watch build
```

### Docker Support

Currently, this project doesn't include Docker support as it's primarily a Unity package. However, the tests can run in any .NET 8.0 environment.

### CI/CD

The project uses GitHub Actions for continuous integration. See `.github/workflows/` for pipeline configurations.

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/Editor/FrankenBit.BoltWire.Tests.Editor.csproj

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

### Test Framework

- **Framework**: NUnit 4.2.2
- **Test SDK**: Microsoft.NET.Test.Sdk 17.11.1
- **Target Framework**: .NET 8.0

### Linting and Code Analysis

```bash
# Run code formatting check
dotnet format --verify-no-changes

# Run code formatting and apply fixes
dotnet format

# Build with code analysis enabled (Debug configuration)
dotnet build -c Debug
```

The project uses Roslyn analyzers during Debug builds via the `CODE_ANALYSIS` preprocessor directive.

## Contributing

We welcome contributions! Please see our [CONTRIBUTING.md](CONTRIBUTING.md) for details on:
- How to submit pull requests
- Coding standards and conventions
- Testing requirements
- Development workflow

By contributing, you agree to abide by our [Code of Conduct](CODE_OF_CONDUCT.md).

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## Contact

- **Author**: FrankenBit
- **Email**: contact@frankenbit.de
- **Website**: https://www.frankenbit.de
- **GitHub**: https://github.com/FrankenBit/BoltWire

## Acknowledgements

- Built for the Unity 3D community
- Inspired by modern .NET dependency injection patterns
- Thanks to all contributors and users providing feedback

## Additional Resources

- [Wiki/Documentation](https://github.com/FrankenBit/BoltWire/wiki)
- [Changelog](https://github.com/FrankenBit/BoltWire/blob/main/CHANGELOG.md)
- [Issue Tracker](https://github.com/FrankenBit/BoltWire/issues)
- [Discussions](https://github.com/FrankenBit/BoltWire/discussions)
