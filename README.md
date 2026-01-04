# peremunoz.WindowsServices

> A developer-friendly .NET library for managing Windows services programmatically.

[![NuGet](https://img.shields.io/nuget/v/peremunoz.WindowsServices.svg?color=blue)](https://www.nuget.org/packages/peremunoz.WindowsServices)
[![NuGet Downloads](https://img.shields.io/nuget/dt/peremunoz.WindowsServices.svg)](https://www.nuget.org/packages/peremunoz.WindowsServices)
[![.NET](https://img.shields.io/badge/.NET-8.0%2B-blueviolet)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)](https://learn.microsoft.com/windows/)
[![License: MIT](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE)

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/O4O41NA0Y7)
---

## Features

- **Install/Update** - Create new services or update existing service configurations
- **Start/Stop/Restart** - Control service lifecycle with timeout support
- **Status Query** - Check service state and retrieve service information
- **Uninstall** - Remove services safely
- **Idempotent** - Operations handle already-running/stopped services gracefully
- **Non-throwing queries** - `TryGetAsync` returns `null` instead of throwing exceptions
- **Multi-targeted** - Supports .NET 8+, .NET 9+, .NET 10+

## Installation

```bash
dotnet add package peremunoz.WindowsServices
```

## Requirements

- **.NET 8.0 or higher** (including .NET 9 and .NET 10)
- **Windows operating system**
- **Administrator privileges** for install/uninstall/control operations

## Supported Platforms

| Platform | Supported Versions |
|----------|-------------------|
| .NET | 8.0, 9.0, 10.0+ |
| Operating System | Windows only |

## Usage

### Basic Service Management

```csharp
using peremunoz.WindowsServices;

var manager = new WindowsServiceManager();

// Install or update a service
var spec = new ServiceSpec(
    Name: "MyTestService",
    ExePath: @"C:\Services\MyService.exe",
    DisplayName: "My Test Service",
    Description: "A test Windows service",
    StartMode: ServiceStartMode.Automatic,
    DelayedAutoStart: false
);

var result = await manager.InstallOrUpdateAsync(spec);
if (result.Success)
{
    Console.WriteLine(result.Message);
}

// Start a service
result = await manager.StartAsync("MyTestService");

// Stop a service
result = await manager.StopAsync("MyTestService");

// Restart a service
result = await manager.RestartAsync("MyTestService");

// Check service status
var status = await manager.GetStatusAsync("MyTestService");
Console.WriteLine($"Service status: {status}");

// Get detailed service info
var info = await manager.TryGetAsync("MyTestService");
if (info != null)
{
    Console.WriteLine($"Name: {info.Name}");
    Console.WriteLine($"Display: {info.DisplayName}");
    Console.WriteLine($"Status: {info.Status}");
}

// Uninstall a service
result = await manager.UninstallIfExistsAsync("MyTestService");
```

### With Timeouts and Cancellation

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));

// Start with custom timeout
var result = await manager.StartAsync(
    "MyTestService", 
    timeout: TimeSpan.FromSeconds(45),
    ct: cts.Token
);

// Restart with longer timeout
result = await manager.RestartAsync(
    "MyTestService",
    timeout: TimeSpan.FromMinutes(2),
    ct: cts.Token
);
```

### Handling Results

```csharp
var result = await manager.StartAsync("MyService");

if (result.Success)
{
    Console.WriteLine($"? {result.Message}");
}
else
{
    Console.WriteLine($"? {result.Code}: {result.Message}");
    if (result.Exception != null)
    {
        Console.WriteLine($"  Exception: {result.Exception.Message}");
    }
}
```

### Common Result Codes

- `OK` - Operation succeeded
- `ALREADY_RUNNING` - Service was already running (Start operation)
- `ALREADY_STOPPED` - Service was already stopped (Stop operation)
- `NOT_INSTALLED` - Service does not exist
- `TIMEOUT` - Operation exceeded the specified timeout
- `WIN32_ERROR` - Windows API error occurred
- `CANCELED` - Operation was canceled via CancellationToken
- `ERROR` - General error occurred

## API Reference

### IServiceManager

Main interface for service management operations.

#### Methods

- `Task<ServiceInfo?> TryGetAsync(string name, CancellationToken ct = default)` - Non-throwing service info query
- `Task<ServiceStatus> GetStatusAsync(string name, CancellationToken ct = default)` - Get service status
- `Task<OpResult> InstallOrUpdateAsync(ServiceSpec spec, CancellationToken ct = default)` - Install or update service
- `Task<OpResult> UninstallIfExistsAsync(string name, CancellationToken ct = default)` - Uninstall service
- `Task<OpResult> StartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)` - Start service
- `Task<OpResult> StopAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)` - Stop service
- `Task<OpResult> RestartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)` - Restart service

### ServiceSpec

Configuration for service installation/update.

- `string Name` - Service name (required)
- `string ExePath` - Path to executable (required)
- `string? Arguments` - Command-line arguments (optional)
- `string? DisplayName` - Display name (optional, defaults to Name)
- `string? Description` - Service description (optional)
- `ServiceStartMode StartMode` - Start mode (default: Automatic)
- `bool DelayedAutoStart` - Delayed auto-start flag (default: false)

### ServiceStatus Enum

- `Unknown` - Status could not be determined
- `NotInstalled` - Service doesn't exist
- `Stopped` - Service is stopped
- `StartPending` - Service is starting
- `StopPending` - Service is stopping
- `Running` - Service is running
- `PausePending` - Service is pausing
- `Paused` - Service is paused
- `ContinuePending` - Service is resuming

## License

See [LICENSE.txt](LICENSE.txt) for license information.

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests.

## Repository

[https://github.com/peremunoz/WindowsServices](https://github.com/peremunoz/WindowsServices)
