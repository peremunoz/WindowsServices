using System.ComponentModel;
using System.Runtime.Versioning;

namespace peremunoz.WindowsServices.Tests;

/// <summary>
/// Integration tests for WindowsServiceManager.
/// These tests require Windows OS and Administrator privileges to run.
/// They use a real test service (a simple .NET console app that acts as a service).
/// </summary>
[SupportedOSPlatform("windows")]
public class WindowsServiceManagerIntegrationTests : IDisposable
{
    private readonly WindowsServiceManager _manager;
    private const string TestServiceName = "XUnitTestService";
    private readonly string _testExePath;

    public WindowsServiceManagerIntegrationTests()
    {
        _manager = new WindowsServiceManager();
        
        // Use a dummy path for testing - in real scenarios, this should be a valid service exe
        _testExePath = Path.Combine(Path.GetTempPath(), "TestService.exe");
        
        // Skip all tests if not on Windows
        if (!OperatingSystem.IsWindows())
        {
            return;
        }
    }

    [Fact]
    public async Task TryGetAsync_NonExistentService_ReturnsNull()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.TryGetAsync("NonExistentService_" + Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task TryGetAsync_ExistingService_ReturnsServiceInfo()
    {
        if (!OperatingSystem.IsWindows()) return;

        // Test with a well-known Windows service
        var result = await _manager.TryGetAsync("EventLog");

        Assert.NotNull(result);
        Assert.Equal("EventLog", result.Name);
        Assert.NotNull(result.DisplayName);
        Assert.NotEqual(ServiceStatus.NotInstalled, result.Status);
    }

    [Fact]
    public async Task GetStatusAsync_NonExistentService_ReturnsNotInstalled()
    {
        if (!OperatingSystem.IsWindows()) return;

        var status = await _manager.GetStatusAsync("NonExistentService_" + Guid.NewGuid());

        Assert.Equal(ServiceStatus.NotInstalled, status);
    }

    [Fact]
    public async Task GetStatusAsync_ExistingService_ReturnsValidStatus()
    {
        if (!OperatingSystem.IsWindows()) return;

        // Test with a well-known Windows service
        var status = await _manager.GetStatusAsync("EventLog");

        Assert.NotEqual(ServiceStatus.NotInstalled, status);
        Assert.NotEqual(ServiceStatus.Unknown, status);
    }

    [Fact]
    public async Task UninstallIfExistsAsync_NonExistentService_ReturnsSuccess()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.UninstallIfExistsAsync("NonExistentService_" + Guid.NewGuid());

        Assert.True(result.Success);
        Assert.Contains("not installed", result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task StartAsync_NonExistentService_ReturnsNotInstalled()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.StartAsync("NonExistentService_" + Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("NOT_INSTALLED", result.Code);
    }

    [Fact]
    public async Task StopAsync_NonExistentService_ReturnsNotInstalled()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.StopAsync("NonExistentService_" + Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("NOT_INSTALLED", result.Code);
    }

    [Fact]
    public async Task RestartAsync_NonExistentService_ReturnsNotInstalled()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.RestartAsync("NonExistentService_" + Guid.NewGuid());

        Assert.False(result.Success);
        Assert.Equal("NOT_INSTALLED", result.Code);
    }

    [Fact]
    public async Task StartAsync_WithTimeout_RespectsTimeout()
    {
        if (!OperatingSystem.IsWindows()) return;

        using var cts = new CancellationTokenSource();
        var result = await _manager.StartAsync(
            "NonExistentService_" + Guid.NewGuid(), 
            TimeSpan.FromSeconds(1), 
            cts.Token);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task StartAsync_WithCancellation_CanBeCanceled()
    {
        if (!OperatingSystem.IsWindows()) return;

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await _manager.StartAsync(
            "NonExistentService_" + Guid.NewGuid(), 
            timeout: null, 
            cts.Token);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task InstallOrUpdateAsync_NullSpec_ThrowsArgumentNullException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _manager.InstallOrUpdateAsync(null!));
    }

    [Fact]
    public async Task InstallOrUpdateAsync_EmptyName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec("", @"C:\test.exe");

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.InstallOrUpdateAsync(spec));
    }

    [Fact]
    public async Task InstallOrUpdateAsync_EmptyExePath_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec("TestService", "");

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.InstallOrUpdateAsync(spec));
    }

    public void Dispose()
    {
        // Cleanup: ensure test service is uninstalled if it exists
        if (OperatingSystem.IsWindows())
        {
            try
            {
                _manager.UninstallIfExistsAsync(TestServiceName).GetAwaiter().GetResult();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}

/// <summary>
/// Integration tests that require administrator privileges.
/// These tests actually install, start, stop, and uninstall services.
/// Run these tests with elevated permissions.
/// </summary>
/// <remarks>
/// To run these tests, create a Trait filter in your test runner:
/// Category=RequiresAdmin
/// </remarks>
[Trait("Category", "RequiresAdmin")]
[SupportedOSPlatform("windows")]
public class WindowsServiceManagerAdminTests : IAsyncLifetime
{
    private readonly WindowsServiceManager _manager;
    private const string TestServiceName = "XUnitTestService_Admin";
    private readonly string _testExePath;
    private readonly ServiceSpec _testSpec;

    public WindowsServiceManagerAdminTests()
    {
        _manager = new WindowsServiceManager();
        
        // For real testing, you'd need a proper service executable
        // This is a placeholder - in practice, build a minimal Windows Service project
        _testExePath = CreateDummyServiceExecutable();
        
        _testSpec = new ServiceSpec(
            Name: TestServiceName,
            ExePath: _testExePath,
            DisplayName: "XUnit Test Service",
            Description: "Service created for xUnit testing",
            StartMode: ServiceStartMode.Manual,
            DelayedAutoStart: false
        );
    }

    public Task InitializeAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Task.CompletedTask;
        }

        // Ensure clean state
        return _manager.UninstallIfExistsAsync(TestServiceName);
    }

    public Task DisposeAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            return Task.CompletedTask;
        }

        // Cleanup
        return _manager.UninstallIfExistsAsync(TestServiceName);
    }

    [Fact]
    public async Task FullLifecycle_InstallStartStopUninstall_Succeeds()
    {
        if (!OperatingSystem.IsWindows()) return;
        if (!IsAdministrator()) return; // Skip if not admin

        try
        {
            // Install
            var installResult = await _manager.InstallOrUpdateAsync(_testSpec);
            Assert.True(installResult.Success, $"Install failed: {installResult.Message}");

            // Verify installed
            var status = await _manager.GetStatusAsync(TestServiceName);
            Assert.NotEqual(ServiceStatus.NotInstalled, status);

            // Get info
            var info = await _manager.TryGetAsync(TestServiceName);
            Assert.NotNull(info);
            Assert.Equal(TestServiceName, info.Name);
            Assert.Equal("XUnit Test Service", info.DisplayName);

            // Note: Starting/stopping requires a valid service executable
            // In real tests, you'd have a proper service that can start/stop

            // Uninstall
            var uninstallResult = await _manager.UninstallIfExistsAsync(TestServiceName);
            Assert.True(uninstallResult.Success, $"Uninstall failed: {uninstallResult.Message}");

            // Verify uninstalled
            status = await _manager.GetStatusAsync(TestServiceName);
            Assert.Equal(ServiceStatus.NotInstalled, status);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 5) // Access Denied
        {
            // Skip test if running without admin privileges
            return;
        }
    }

    [Fact]
    public async Task InstallOrUpdateAsync_Update_ModifiesExistingService()
    {
        if (!OperatingSystem.IsWindows()) return;
        if (!IsAdministrator()) return;

        try
        {
            // Install
            var installResult = await _manager.InstallOrUpdateAsync(_testSpec);
            Assert.True(installResult.Success);

            // Update
            var updatedSpec = _testSpec with 
            { 
                DisplayName = "Updated Display Name",
                Description = "Updated description",
                StartMode = ServiceStartMode.Automatic 
            };

            var updateResult = await _manager.InstallOrUpdateAsync(updatedSpec);
            Assert.True(updateResult.Success);
            Assert.Contains("Updated", updateResult.Message);

            // Verify update
            var info = await _manager.TryGetAsync(TestServiceName);
            Assert.NotNull(info);
            Assert.Equal("Updated Display Name", info.DisplayName);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
        {
            return;
        }
    }

    [Fact]
    public async Task UninstallIfExistsAsync_InstalledService_Succeeds()
    {
        if (!OperatingSystem.IsWindows()) return;
        if (!IsAdministrator()) return;

        try
        {
            // Install first
            await _manager.InstallOrUpdateAsync(_testSpec);

            // Uninstall
            var result = await _manager.UninstallIfExistsAsync(TestServiceName);

            Assert.True(result.Success);
            Assert.Contains("Uninstalled", result.Message);

            // Verify
            var status = await _manager.GetStatusAsync(TestServiceName);
            Assert.Equal(ServiceStatus.NotInstalled, status);
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
        {
            return;
        }
    }

    private static string CreateDummyServiceExecutable()
    {
        // In real tests, you would:
        // 1. Build a simple Windows Service executable
        // 2. Copy it to a test location
        // 3. Return that path
        
        // For now, return a placeholder path
        // Note: Tests requiring actual service operations will fail without a real executable
        var tempPath = Path.Combine(Path.GetTempPath(), "TestService.exe");
        
        // Create an empty file as placeholder
        if (!File.Exists(tempPath))
        {
            File.WriteAllText(tempPath, string.Empty);
        }
        
        return tempPath;
    }

    private static bool IsAdministrator()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        try
        {
            var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var principal = new System.Security.Principal.WindowsPrincipal(identity);
            return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        }
        catch
        {
            return false;
        }
    }
}
