# Contributing to peremunoz.WindowsServices

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Code of Conduct

Be respectful and considerate of others. We want this to be a welcoming community for everyone.

## How to Contribute

### Reporting Issues

- Use the GitHub issue tracker
- Check if the issue already exists
- Provide a clear description of the problem
- Include steps to reproduce
- Specify your environment (.NET version, Windows version)
- Include relevant code samples or error messages

### Suggesting Features

- Open a GitHub issue with the "enhancement" label
- Describe the use case and expected behavior
- Explain why this would be useful to most users
- Consider if it fits the project's scope (Windows Service management)

### Pull Requests

1. **Fork** the repository
2. **Create a branch** for your feature or fix
   ```bash
   git checkout -b feature/my-new-feature
   ```
3. **Make your changes** following the coding guidelines
4. **Add tests** for new functionality
5. **Ensure all tests pass**
   ```bash
   dotnet test
   ```
6. **Update documentation** (README.md, XML docs, CHANGELOG.md)
7. **Commit with clear messages**
   ```bash
   git commit -m "Add feature: description of feature"
   ```
8. **Push** to your fork
   ```bash
   git push origin feature/my-new-feature
   ```
9. **Create a Pull Request** with a clear description

## Development Guidelines

### Coding Style

- Follow existing code style and conventions
- Use **modern C# features** appropriately (records, pattern matching, etc.)
- Enable and respect **nullable reference types**
- Use **async/await** for I/O operations
- Apply **ConfigureAwait(false)** in library code
- Keep methods focused and single-purpose
- Prefer **immutability** where possible

### Testing

- Write tests for all new functionality
- Maintain or improve code coverage
- Use descriptive test names that explain the scenario
- Test both success and failure paths
- Include edge cases and error conditions
- Tests should be:
  - **Fast** - Unit tests < 100ms
  - **Isolated** - No dependencies between tests
  - **Repeatable** - Same result every time
  - **Self-validating** - Pass or fail clearly
  - **Timely** - Written with or before code

### Test Categories

- **Unit Tests** - Test individual components in isolation
- **Integration Tests** - Test Windows Service interactions (require Windows)
- **Admin Tests** - Tests requiring administrator privileges (tagged with `[Trait("Category", "RequiresAdmin")]`)

### Documentation

- Add **XML documentation** to all public APIs
- Update **README.md** for user-facing changes
- Update **CHANGELOG.md** following Keep a Changelog format
- Include **code examples** for new features
- Document **breaking changes** clearly

### Commit Messages

Follow conventional commits format:

```
<type>(<scope>): <subject>

<body>

<footer>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `test`: Adding or updating tests
- `refactor`: Code changes that neither fix bugs nor add features
- `perf`: Performance improvements
- `chore`: Maintenance tasks

Examples:
```
feat(service): Add support for service dependencies

Add ServiceDependencies property to ServiceSpec to allow
specifying services that must start before this service.

Closes #42
```

```
fix(uninstall): Handle service marked for deletion

Treat ERROR_SERVICE_MARKED_FOR_DELETE as success in
UninstallIfExistsAsync to prevent spurious errors.

Fixes #38
```

### Project Structure

```
peremunoz.WindowsServices/
??? peremunoz.WindowsServices/      # Main library
?   ??? IServiceManager.cs          # Public interface
?   ??? WindowsServiceManager.cs    # Implementation
?   ??? ServiceSpec.cs              # Configuration types
?   ??? OpResult.cs                 # Result types
?   ??? Native/                     # P/Invoke declarations
?   ??? Helpers/                    # Helper classes
?   ??? Internals/                  # Internal utilities
??? peremunoz.WindowsServices.Tests/# Test project
??? README.md                       # User documentation
??? CHANGELOG.md                    # Version history
??? CONTRIBUTING.md                 # This file
??? LICENSE.txt                     # MIT License
```

### Building

```bash
# Restore dependencies
dotnet restore

# Build all targets
dotnet build

# Run tests (non-admin)
dotnet test --filter "Category!=RequiresAdmin"

# Run all tests (requires admin)
dotnet test

# Create NuGet package
dotnet pack -c Release
```

### Multi-Targeting

The library targets:
- .NET 10.0
- .NET 9.0
- .NET 8.0

When adding code, ensure it compiles for all targets. Use conditional compilation if needed:

```csharp
#if NET8_0_OR_GREATER
    // Modern .NET code
#else
    // Fallback for older frameworks
#endif
```

## What to Contribute

### Good First Issues

Look for issues tagged `good first issue` - these are simpler tasks for newcomers.

### High-Priority Areas

1. **Additional service configuration options**
   - Service accounts (LocalSystem, LocalService, NetworkService)
   - Service recovery options
   - Failure actions

2. **Service enumeration**
   - List all services
   - Filter by status, start type, etc.

3. **Advanced features**
   - Service dependencies
   - Pause/Resume support
   - Service trigger support

4. **Documentation improvements**
   - More usage examples
   - Troubleshooting guide
   - Best practices guide

5. **Test coverage**
   - More edge case testing
   - Performance benchmarks
   - Real service integration tests

### Out of Scope

- Non-Windows platforms (this is Windows-specific)
- Service creation from code (use `BackgroundService` with `AddWindowsService()`)
- GUI applications
- Non-service-related Windows management

## Questions?

Open an issue with the `question` label or start a discussion on GitHub.

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

Thank you for contributing to peremunoz.WindowsServices! ??
