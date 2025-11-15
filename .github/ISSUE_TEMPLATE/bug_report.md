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
1. Set up environment with '...'
2. Configure services using '...'
3. Call method '...'
4. See error

## Expected Behavior
A clear and concise description of what you expected to happen.

## Actual Behavior
A clear and concise description of what actually happened.

## Code Sample
If applicable, provide a minimal code sample that reproduces the issue:

```csharp
// Your code here
var services = new ServiceCollection();
services.AddSingleton<IService, ServiceImplementation>();
var provider = services.BuildServiceProvider();
// ...
```

## Error Messages / Stack Trace
If applicable, include any error messages or stack traces:

```
Paste error messages or stack trace here
```

## Environment Details
Please provide the following information:

- **BoltWire Version**: [e.g., 1.0.0-pre.1]
- **Unity Version**: [e.g., 6.0.21f1]
- **.NET SDK Version** (for development): [e.g., .NET 8.0]
- **Operating System**: [e.g., Windows 11, macOS 14, Ubuntu 22.04]
- **IDE**: [e.g., Visual Studio 2022, Rider 2023.3, VS Code]
- **Target Platform**: [e.g., Windows Standalone, Android, iOS]

## Additional Context
Add any other context about the problem here. This might include:
- Is this a regression (did it work in a previous version)?
- Does this only happen in specific scenarios?
- Have you found any workarounds?
- Screenshots or GIFs showing the issue

## Possible Solution
If you have ideas on how to fix this issue, please describe them here (optional).

## Related Issues
Link to any related issues or pull requests (if applicable).
