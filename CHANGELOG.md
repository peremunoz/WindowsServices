# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-01-04

### Added
- Initial release of peremunoz.WindowsServices
- `IServiceManager` interface for Windows Service management
- `WindowsServiceManager` implementation with full SCM API integration
- Service installation and update capabilities
- Service uninstallation with safe cleanup
- Service lifecycle control (Start, Stop, Restart)
- Service status queries (TryGetAsync, GetStatusAsync)
- Comprehensive service configuration via `ServiceSpec`
  - Service name, display name, and description
  - Executable path and command-line arguments
  - Start mode (Automatic, Manual, Disabled)
  - Delayed auto-start support
- Robust operation results with `OpResult` type
  - Success/failure indication
  - Error codes for programmatic handling
  - Exception details for debugging
- Service status enumeration with all Windows service states
- Idempotent operations (safe to call multiple times)
- Non-throwing query methods (`TryGetAsync`)
- Timeout support for all service control operations
- Cancellation token support for async operations
- Multi-targeting support (.NET 8, .NET 9, .NET 10)
- Complete XML documentation for IntelliSense
- 96+ comprehensive unit and integration tests
- Platform-specific guards (Windows-only)

### Features
- **Zero dependencies** (except System.ServiceProcess.ServiceController)
- **Type-safe** with modern C# records
- **Async/await** throughout
- **ConfigureAwait(false)** for library usage
- **Nullable reference types** enabled
- **Platform analyzers** to prevent misuse on non-Windows platforms

### Documentation
- Comprehensive README with usage examples
- XML documentation for all public APIs
- Multi-targeting strategy document
- Test documentation and guidelines

### Technical
- Direct P/Invoke to Windows SCM APIs (advapi32.dll)
- Safe handle management for resource cleanup
- Proper error handling with Win32Exception
- Image path builder for executable paths with spaces
- Service configuration helpers for advanced settings

## [Unreleased]

### Planned
- Service dependency management
- Service recovery options
- Service failure actions
- Service accounts (LocalSystem, LocalService, NetworkService, custom)
- Query all services
- Service enumeration and filtering
- Performance counters support

---

[1.0.0]: https://github.com/peremunoz/WindowsServices/releases/tag/v1.0.0
