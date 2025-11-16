# BoltWire

> Another scary dependency injection framework for Unity 3D

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## About

BoltWire is a lightweight, powerful dependency injection (DI) framework specifically designed for Unity 3D game development. Built with C#/.NET, BoltWire provides a clean, intuitive API for managing dependencies in your Unity projects, helping you write more maintainable, testable, and decoupled code.

Whether you're building a small indie game or a large-scale production, BoltWire offers the flexibility and performance you need without the overhead of heavier DI containers.

## Features

- **Unity-First Design**: Built specifically for Unity 3D with full support for Unity's lifecycle and serialization
- **Lightweight & Fast**: Minimal overhead with optimized resolution and caching
- **Flexible Registration**: Support for transient, scoped, and singleton lifetimes
- **Constructor Injection**: Automatic constructor parameter resolution
- **Factory Support**: Register factory methods for complex object creation
- **Scoped Containers**: Create child scopes for isolated dependency graphs
- **Assembly Definition Files**: Modular design with proper asmdef structure
- **Collection Support**: Inject multiple implementations of the same interface
- **IStartable Interface**: Automatic initialization of registered services
- **Modern C#**: Built with nullable reference types and modern C# features
- **.NET Standard Compatible**: Works with Unity 6.0+ and .NET projects

## Getting Started

### Prerequisites

- **Unity**: Unity 6.0 or later (21f1 release or newer)
- **.NET**: .NET Standard 2.1 compatible
- **C#**: C# 9.0 or later recommended

### Installation

#### Option 1: Unity Package Manager (Git URL)

1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button and select `Add package from git URL...`
3. Enter: `https://github.com/swalex/BoltWire.git`
4. Click `Add`

#### Option 2: Manual Installation

1. Clone or download this repository
2. Copy the package into your Unity project's `Packages` folder
3. Unity will automatically detect and import the package

#### Option 3: Package Manager (Local)

1. Clone this repository to your local machine
2. In Unity, open Package Manager
3. Click `+` > `Add package from disk...`
4. Navigate to the cloned repository and select `package.json`

### Quickstart

#### Basic Usage in Unity

```csharp
using FrankenBit.BoltWire;
using UnityEngine;

// Define your services
public interface IGameService
{
    void Initialize();
}

public class GameService : IGameService
{
    public void Initialize()
    {
        Debug.Log("Game service initialized!");
    }
}

// Register services in a MonoBehaviour
public class GameInstaller : MonoBehaviour
{
    private IServiceProvider _serviceProvider;

    private void Awake()
    {
        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<IGameService, GameService>();
        services.AddTransient<PlayerController>();
        
        // Build the container
        _serviceProvider = services.BuildServiceProvider();
        
        // Resolve and use services
        var gameService = _serviceProvider.GetService<IGameService>();
        gameService?.Initialize();
    }
}
```

#### Using with .NET CLI

For standalone .NET projects or testing outside Unity:

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test
```

## Configuration

### Unity Configuration

BoltWire integrates seamlessly with Unity's project structure. The package includes Assembly Definition Files (asmdef) for proper code separation:

- `FrankenBit.BoltWire.asmdef` - Runtime assembly
- `FrankenBit.BoltWire.Editor.Tests.asmdef` - Editor test assembly

To reference BoltWire in your own assemblies:

1. Create or open your Assembly Definition file
2. Add `FrankenBit.BoltWire` to the Assembly Definition References list

### Environment Variables

While BoltWire doesn't require environment variables, you can configure build behaviors:

- `UNITY_EDITOR` - Automatically defined by Unity for editor-specific code
- `UNITY_INCLUDE_TESTS` - Include test assemblies in builds (Unity setting)

### Example Configuration

```csharp
// Advanced configuration example
var services = new ServiceCollection();

// Singleton - one instance for the entire application lifetime
services.AddSingleton<IGameConfig, GameConfig>();

// Scoped - one instance per scope (useful for level/scene isolation)
services.AddScoped<ILevelManager, LevelManager>();

// Transient - new instance every time it's requested
services.AddTransient<IEnemy, Enemy>();

// Factory registration
services.AddSingleton<IEnemyFactory>(provider => 
    new EnemyFactory(provider.GetService<IGameConfig>()));

// Collection registration
services.AddSingleton<IGameSystem, PhysicsSystem>();
services.AddSingleton<IGameSystem, RenderSystem>();
services.AddSingleton<IGameSystem, AudioSystem>();

var provider = services.BuildServiceProvider();
```

## Usage

### Running in Unity

1. Create a new script that inherits from `MonoBehaviour`
2. Set up your service registration in `Awake()` or `Start()`
3. Attach the script to a GameObject in your scene
4. Access services throughout your game code

### Creating Scopes

```csharp
public class LevelManager : MonoBehaviour
{
    private IServiceProvider _rootProvider;
    private IScope _levelScope;

    public void LoadLevel()
    {
        // Create a new scope for level-specific services
        _levelScope = _rootProvider.CreateScope();
        var scopedProvider = _levelScope.ServiceProvider;
        
        // Resolve level-specific services
        var levelData = scopedProvider.GetService<ILevelData>();
    }

    public void UnloadLevel()
    {
        // Dispose the scope to clean up level services
        _levelScope?.Dispose();
    }
}
```

### Working with Collections

```csharp
// Register multiple implementations
services.AddSingleton<IGameSystem, PhysicsSystem>();
services.AddSingleton<IGameSystem, AudioSystem>();

// Resolve all implementations
var systems = provider.GetServices<IGameSystem>();
foreach (var system in systems)
{
    system.Initialize();
}
```

### IStartable Interface

Services implementing `IStartable` are automatically started:

```csharp
public class GameService : IGameService, IStartable
{
    public void Start()
    {
        // Called automatically when the service provider is built
        Debug.Log("Service started!");
    }
}
```

## Development

### Branching Strategy

We follow a feature-branch workflow:

- `main` - Stable production code
- `feat/feature-name` - New features
- `fix/bug-name` - Bug fixes
- `docs/description` - Documentation updates
- `chore/description` - Maintenance tasks

### Coding Style

BoltWire follows standard C# conventions with Unity-specific considerations:

- **Use `dotnet format`** for automatic code formatting
- **EditorConfig**: The project includes an `.editorconfig` file - ensure your IDE respects it
- **Nullable Reference Types**: Enable and use nullable reference types (`#nullable enable`)
- **Modern C#**: Use pattern matching, expression-bodied members, and other modern features
- **Unity Naming**: Follow Unity conventions for MonoBehaviour methods (PascalCase)
- **Async/Await**: Avoid `async/await` in hot paths; use Unity coroutines where appropriate

#### Assembly Definitions

- Keep runtime code in the `Runtime` assembly
- Keep editor-only code separate (use `Editor` folders)
- Never reference editor assemblies from runtime code
- Use asmdef files to define clear assembly boundaries

### Running Locally

#### Unity Development

1. Open the project in Unity 6.0 or later
2. The package should be automatically recognized
3. Make your changes in the `Runtime` or `Tests` folders
4. Test your changes using the Unity Test Runner

#### .NET Development

```bash
# Clone the repository
git clone https://github.com/swalex/BoltWire.git
cd BoltWire

# Restore dependencies
dotnet restore

# Build
dotnet build

# Run tests
dotnet test
```

### Docker/CI Notes

While BoltWire is primarily developed in Unity, the .NET components can be built in CI/CD pipelines:

```dockerfile
# Example Dockerfile for CI
FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
COPY . .
RUN dotnet restore
RUN dotnet build --configuration Release
RUN dotnet test --no-build --configuration Release
```

**Note**: Unity-specific features require the Unity Editor and cannot be fully tested in Docker without a Unity build server setup.

## Testing

### Unity Test Runner

BoltWire includes comprehensive tests for Unity:

1. Open Unity Test Runner (`Window > General > Test Runner`)
2. Switch between EditMode and PlayMode tabs
3. Click `Run All` to execute tests

#### EditMode Tests
- Fast, run in the editor
- Test core DI functionality
- No scene required

#### PlayMode Tests
- Run in Play mode
- Test Unity-specific integrations
- May require scene setup

### .NET Testing

Run tests using the .NET CLI:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run tests in a specific project
dotnet test Tests/Editor/FrankenBit.BoltWire.Tests.Editor.csproj

# Run with coverage (requires coverlet)
dotnet test /p:CollectCoverage=true
```

### Writing Tests

Follow existing test patterns:

```csharp
using NUnit.Framework;

[TestFixture]
public class ServiceRegistrationTests
{
    [Test]
    public void AddSingleton_RegistersService_Successfully()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddSingleton<IMyService, MyService>();
        var provider = services.BuildServiceProvider();
        
        // Assert
        Assert.IsNotNull(provider.GetService<IMyService>());
    }
}
```

## Contributing

We welcome contributions! Please read our [CONTRIBUTING.md](CONTRIBUTING.md) for details on:

- How to submit pull requests
- Coding standards
- Commit message conventions
- Testing requirements
- Code review process

Also, please review our [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

The original BoltWire project is Copyright (c) 2024 FrankenBit. This fork is maintained by swalex.

## Contact

- **Issues**: [GitHub Issues](https://github.com/swalex/BoltWire/issues)
- **Discussions**: [GitHub Discussions](https://github.com/swalex/BoltWire/discussions)
- **Original Project**: [FrankenBit/BoltWire](https://github.com/FrankenBit/BoltWire)

## Acknowledgements

- Original BoltWire framework by [FrankenBit](https://www.frankenbit.de)
- Inspired by ASP.NET Core's dependency injection system
- Built for the Unity community

