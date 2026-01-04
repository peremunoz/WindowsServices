# peremunoz.WindowsServices.Tests

Test suite for the Windows Services management library.

## Test Categories

### Unit Tests
These tests don't require Windows or administrator privileges:
- `GuardTests` - Tests for input validation
- `ServiceSpecTests` - Tests for service specification records
- `OpResultTests` - Tests for operation result handling
- `ServiceInfoTests` - Tests for service information records
- `ServiceStatusTests` - Tests for service status enum
- `ServiceStartModeTests` - Tests for service start mode enum
- `ImagePathBuilderTests` - Tests for executable path formatting

### Integration Tests
These tests require **Windows OS** but not administrator privileges:
- `WindowsServiceManagerIntegrationTests` - Tests that query existing services
- `WindowsServiceManagerEdgeCasesTests` - Edge case and error handling tests

### Admin Tests
These tests require **Windows OS** and **Administrator privileges**:
- `WindowsServiceManagerAdminTests` - Full lifecycle tests (install, start, stop, uninstall)

## Running Tests

### Run All Tests (Including Admin Tests)
Run Visual Studio or your test runner as Administrator:

```bash
# PowerShell (Run as Administrator)
dotnet test
```

### Run Only Non-Admin Tests
Filter out tests that require admin privileges:

```bash
dotnet test --filter "Category!=RequiresAdmin"
```

### Run Only Admin Tests
```bash
# PowerShell (Run as Administrator)
dotnet test --filter "Category=RequiresAdmin"
```

### Visual Studio Test Explorer
1. Open Test Explorer (Test > Test Explorer)
2. Right-click a test or test class
3. Select "Run" or "Debug"

To run admin tests, start Visual Studio as Administrator.

## Test Requirements

### For All Tests
- Windows Operating System
- .NET 10 SDK

### For Admin Tests
- Administrator privileges
- Valid service executable (currently uses a placeholder)

## Creating a Real Test Service

The admin tests currently use a placeholder executable. To test real service operations:

1. Create a simple Windows Service project:
```csharp
// Program.cs
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
await host.RunAsync();

// Worker.cs
public class Worker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

2. Publish the service:
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

3. Update `CreateDummyServiceExecutable()` in `WindowsServiceManagerAdminTests` to return the path to your published executable.

## Test Coverage

Current test coverage includes:

- ? Input validation (null, empty, whitespace)
- ? Service specification creation and modification
- ? Operation result handling
- ? Service status queries
- ? Non-existent service handling
- ? Edge cases (long names, special characters, timeouts)
- ? Cancellation token support
- ? All enum values
- ? Record equality and modifications
- ?? Full lifecycle (requires admin and valid service executable)
- ?? Start/Stop operations (requires admin and running service)

## CI/CD Considerations

For continuous integration:

1. **Non-Admin Tests**: Can run on any Windows CI agent
2. **Admin Tests**: Require elevated privileges
   - Option A: Run on a dedicated Windows agent with admin rights
   - Option B: Skip admin tests in CI and run them manually
   - Option C: Use a test service account with appropriate permissions

Example GitHub Actions workflow:
```yaml
name: Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      - name: Run Unit Tests
        run: dotnet test --filter "Category!=RequiresAdmin"
```

## Troubleshooting

### "Access is denied" errors
Run your test runner as Administrator.

### "Service does not exist" errors in admin tests
Ensure the test service executable path is correct and the file exists.

### Tests skip on non-Windows
This is expected. All service management tests require Windows.

### PlatformNotSupportedException
Ensure you're running on Windows. Some tests automatically skip on non-Windows platforms.
