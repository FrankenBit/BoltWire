---
name: Bug Report
about: Create a report to help us improve BoltWire
title: '[BUG] '
labels: bug
assignees: ''
---

## Bug Description

A clear and concise description of what the bug is.

## Steps to Reproduce

Steps to reproduce the behavior:

1. Go to '...'
2. Create a service with '...'
3. Resolve the service '...'
4. See error

## Expected Behavior

A clear and concise description of what you expected to happen.

## Actual Behavior

A clear and concise description of what actually happened.

## Environment

Please complete the following information:

- **Unity Version**: [e.g., 6.0.21f1, 2022.3.15f1]
- **BoltWire Version**: [e.g., 1.0.0-pre.1]
- **.NET Version**: [e.g., .NET Standard 2.1, .NET 8.0]
- **Operating System**: [e.g., Windows 11, macOS 14.2, Ubuntu 22.04]
- **Editor/Build**: [Editor or Build, if Build specify platform: Windows/Mac/Linux/iOS/Android/WebGL]

## Code Sample

If applicable, provide a minimal code sample that reproduces the issue:

```csharp
// Your code here
var services = new ServiceCollection();
services.AddSingleton<IMyService, MyService>();
var provider = services.BuildServiceProvider();
// ...
```

## Logs and Stack Traces

If applicable, add error logs or stack traces:

```
Paste your logs here
```

## Screenshots

If applicable, add screenshots to help explain your problem.

## Additional Context

Add any other context about the problem here, such as:
- Does this occur in the Unity Editor, in builds, or both?
- Does this happen with specific service lifetimes (Singleton, Scoped, Transient)?
- Are there any patterns or specific scenarios where the bug appears?
- Have you found any workarounds?

## Possible Solution

If you have ideas about what might be causing the issue or how to fix it, please share them here (optional).
