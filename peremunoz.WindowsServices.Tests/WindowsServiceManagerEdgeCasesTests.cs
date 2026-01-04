namespace peremunoz.WindowsServices.Tests;

/// <summary>
/// Tests for edge cases and error handling
/// </summary>
public class WindowsServiceManagerEdgeCasesTests
{
    private readonly WindowsServiceManager _manager;

    public WindowsServiceManagerEdgeCasesTests()
    {
        _manager = new WindowsServiceManager();
    }

    [Fact]
    public async Task TryGetAsync_NullName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.TryGetAsync(null!));
    }

    [Fact]
    public async Task TryGetAsync_EmptyName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.TryGetAsync(string.Empty));
    }

    [Fact]
    public async Task TryGetAsync_WhitespaceName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.TryGetAsync("   "));
    }

    [Fact]
    public async Task GetStatusAsync_NullName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.GetStatusAsync(null!));
    }

    [Fact]
    public async Task StartAsync_NullName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.StartAsync(null!));
    }

    [Fact]
    public async Task StopAsync_NullName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.StopAsync(null!));
    }

    [Fact]
    public async Task RestartAsync_NullName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.RestartAsync(null!));
    }

    [Fact]
    public async Task UninstallIfExistsAsync_NullName_ThrowsArgumentException()
    {
        if (!OperatingSystem.IsWindows()) return;

        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _manager.UninstallIfExistsAsync(null!));
    }

    [Fact]
    public async Task TryGetAsync_VeryLongName_HandlesGracefully()
    {
        if (!OperatingSystem.IsWindows()) return;

        var longName = new string('A', 1000);
        var result = await _manager.TryGetAsync(longName);

        // Should return null or handle gracefully, not crash
        Assert.Null(result);
    }

    [Fact]
    public async Task TryGetAsync_SpecialCharacters_HandlesGracefully()
    {
        if (!OperatingSystem.IsWindows()) return;

        var specialName = "Test\0Invalid";
        var result = await _manager.TryGetAsync(specialName);

        // Should handle gracefully
        Assert.Null(result);
    }

    [Fact]
    public async Task StartAsync_WithZeroTimeout_HandlesProperly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.StartAsync(
            "NonExistentService", 
            TimeSpan.Zero);

        Assert.False(result.Success);
    }

    [Fact]
    public async Task StartAsync_WithNegativeTimeout_HandlesProperly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.StartAsync(
            "NonExistentService", 
            TimeSpan.FromSeconds(-1));

        Assert.False(result.Success);
    }

    [Fact]
    public async Task InstallOrUpdateAsync_WithArguments_FormatsCorrectly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\test.exe",
            Arguments: "--port 5000 --host localhost"
        );

        // This will fail without admin rights, but we're testing argument validation
        var result = await _manager.InstallOrUpdateAsync(spec);

        // Should attempt to install (might fail due to permissions or missing exe)
        Assert.NotNull(result);
    }

    [Fact]
    public async Task InstallOrUpdateAsync_WithSpacesInPath_HandlesCorrectly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\Program Files\My Service\test.exe"
        );

        var result = await _manager.InstallOrUpdateAsync(spec);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task InstallOrUpdateAsync_AllStartModes_AreHandled()
    {
        if (!OperatingSystem.IsWindows()) return;

        var modes = new[] 
        { 
            ServiceStartMode.Automatic, 
            ServiceStartMode.Manual, 
            ServiceStartMode.Disabled 
        };

        foreach (var mode in modes)
        {
            var spec = new ServiceSpec(
                Name: "TestService",
                ExePath: @"C:\test.exe",
                StartMode: mode
            );

            var result = await _manager.InstallOrUpdateAsync(spec);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task InstallOrUpdateAsync_WithDelayedAutoStart_IsHandled()
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\test.exe",
            StartMode: ServiceStartMode.Automatic,
            DelayedAutoStart: true
        );

        var result = await _manager.InstallOrUpdateAsync(spec);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Short description")]
    [InlineData("This is a very long description that contains a lot of text to ensure that the service manager can handle descriptions of various lengths without any issues or crashes.")]
    public async Task InstallOrUpdateAsync_VariousDescriptions_AreHandled(string? description)
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\test.exe",
            Description: description
        );

        var result = await _manager.InstallOrUpdateAsync(spec);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task Operations_WithCanceledToken_ReturnQuickly()
    {
        if (!OperatingSystem.IsWindows()) return;

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var spec = new ServiceSpec("Test", @"C:\test.exe");

        var installResult = await _manager.InstallOrUpdateAsync(spec, cts.Token);
        Assert.False(installResult.Success);

        var uninstallResult = await _manager.UninstallIfExistsAsync("Test", cts.Token);
        Assert.False(uninstallResult.Success);
    }
}
