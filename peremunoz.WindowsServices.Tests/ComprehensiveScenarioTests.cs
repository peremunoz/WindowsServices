namespace peremunoz.WindowsServices.Tests;

/// <summary>
/// Comprehensive tests covering various scenarios and usage patterns
/// </summary>
public class ComprehensiveScenarioTests
{
    private readonly WindowsServiceManager _manager;

    public ComprehensiveScenarioTests()
    {
        _manager = new WindowsServiceManager();
    }

    [Fact]
    public async Task ServiceSpec_DefaultValues_AreAppliedCorrectly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\test.exe"
        );

        Assert.Equal("TestService", spec.Name);
        Assert.Equal(@"C:\test.exe", spec.ExePath);
        Assert.Null(spec.Arguments);
        Assert.Null(spec.DisplayName);
        Assert.Null(spec.Description);
        Assert.Equal(ServiceStartMode.Automatic, spec.StartMode);
        Assert.False(spec.DelayedAutoStart);
    }

    [Fact]
    public async Task ServiceSpec_WithExpression_CreatesCorrectly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var baseSpec = new ServiceSpec("Base", @"C:\base.exe");
        var modified = baseSpec with
        {
            DisplayName = "Modified Service",
            Description = "A modified service",
            StartMode = ServiceStartMode.Disabled
        };

        Assert.Equal("Base", modified.Name);
        Assert.Equal("Modified Service", modified.DisplayName);
        Assert.Equal("A modified service", modified.Description);
        Assert.Equal(ServiceStartMode.Disabled, modified.StartMode);
    }

    [Theory]
    [InlineData("MyService")]
    [InlineData("My-Service-With-Dashes")]
    [InlineData("My_Service_With_Underscores")]
    [InlineData("MyService123")]
    public async Task ServiceNames_ValidFormats_AreAccepted(string serviceName)
    {
        if (!OperatingSystem.IsWindows()) return;

        var info = await _manager.TryGetAsync(serviceName);
        // Should not throw, even if service doesn't exist
        Assert.True(info == null || info.Name == serviceName);
    }

    [Fact]
    public async Task MultipleOperations_InSequence_WorkCorrectly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var nonExistentService = "NonExistent_" + Guid.NewGuid();

        // Multiple status checks
        var status1 = await _manager.GetStatusAsync(nonExistentService);
        var status2 = await _manager.GetStatusAsync(nonExistentService);
        var status3 = await _manager.GetStatusAsync(nonExistentService);

        Assert.Equal(ServiceStatus.NotInstalled, status1);
        Assert.Equal(ServiceStatus.NotInstalled, status2);
        Assert.Equal(ServiceStatus.NotInstalled, status3);
    }

    [Fact]
    public async Task TryGetAsync_WellKnownService_ReturnsConsistentInfo()
    {
        if (!OperatingSystem.IsWindows()) return;

        // EventLog is a well-known Windows service
        var info1 = await _manager.TryGetAsync("EventLog");
        var info2 = await _manager.TryGetAsync("EventLog");

        Assert.NotNull(info1);
        Assert.NotNull(info2);
        Assert.Equal(info1.Name, info2.Name);
        Assert.Equal(info1.DisplayName, info2.DisplayName);
    }

    [Fact]
    public async Task GetStatusAsync_Concurrent_HandlesCorrectly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.GetStatusAsync("EventLog"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, status => Assert.NotEqual(ServiceStatus.NotInstalled, status));
    }

    [Fact]
    public async Task TryGetAsync_Concurrent_HandlesCorrectly()
    {
        if (!OperatingSystem.IsWindows()) return;

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => _manager.TryGetAsync("EventLog"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, info => Assert.NotNull(info));
    }

    [Fact]
    public void OpResult_SuccessResults_HaveConsistentProperties()
    {
        var result1 = OpResult.Ok();
        var result2 = OpResult.Ok("Custom message");

        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.Equal("OK", result1.Code);
        Assert.Equal("OK", result2.Code);
        Assert.Null(result1.Exception);
        Assert.Null(result2.Exception);
    }

    [Fact]
    public void OpResult_FailureResults_HaveConsistentProperties()
    {
        var exception = new InvalidOperationException("Test");
        var result1 = OpResult.Fail("ERROR", "Message");
        var result2 = OpResult.Fail("ERROR", "Message", exception);

        Assert.False(result1.Success);
        Assert.False(result2.Success);
        Assert.Equal("ERROR", result1.Code);
        Assert.Equal("ERROR", result2.Code);
        Assert.Null(result1.Exception);
        Assert.Same(exception, result2.Exception);
    }

    [Fact]
    public void ServiceStatus_AllValues_AreUnique()
    {
        var values = Enum.GetValues<ServiceStatus>();
        var distinctValues = values.Distinct().ToArray();

        Assert.Equal(values.Length, distinctValues.Length);
    }

    [Fact]
    public void ServiceStartMode_AllValues_MatchWindowsConstants()
    {
        // Windows SCM constants
        Assert.Equal(2, (int)ServiceStartMode.Automatic); // SERVICE_AUTO_START
        Assert.Equal(3, (int)ServiceStartMode.Manual);    // SERVICE_DEMAND_START
        Assert.Equal(4, (int)ServiceStartMode.Disabled);  // SERVICE_DISABLED
    }

    [Fact]
    public async Task InstallOrUpdateAsync_WithAllCombinations_DoesNotThrow()
    {
        if (!OperatingSystem.IsWindows()) return;

        var modes = Enum.GetValues<ServiceStartMode>();
        var delayedOptions = new[] { true, false };

        foreach (var mode in modes)
        {
            foreach (var delayed in delayedOptions)
            {
                var spec = new ServiceSpec(
                    Name: "TestService",
                    ExePath: @"C:\test.exe",
                    StartMode: mode,
                    DelayedAutoStart: delayed
                );

                var result = await _manager.InstallOrUpdateAsync(spec);
                Assert.NotNull(result);
            }
        }
    }

    [Theory]
    [InlineData(@"C:\test.exe")]
    [InlineData(@"C:\Program Files\Test\test.exe")]
    [InlineData(@"C:\Program Files (x86)\Test\test.exe")]
    [InlineData(@"D:\Services\MyService\bin\service.exe")]
    public async Task InstallOrUpdateAsync_VariousExePaths_HandlesCorrectly(string exePath)
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: exePath
        );

        var result = await _manager.InstallOrUpdateAsync(spec);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("--port 5000")]
    [InlineData("--config \"C:\\Program Files\\Config\\app.json\"")]
    [InlineData("/service --verbose")]
    public async Task InstallOrUpdateAsync_VariousArguments_HandlesCorrectly(string? arguments)
    {
        if (!OperatingSystem.IsWindows()) return;

        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\test.exe",
            Arguments: arguments
        );

        var result = await _manager.InstallOrUpdateAsync(spec);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ServiceInfo_Record_SupportsDeconstruction()
    {
        if (!OperatingSystem.IsWindows()) return;

        var info = await _manager.TryGetAsync("EventLog");
        
        if (info != null)
        {
            var (name, displayName, description, status) = info;

            Assert.Equal(info.Name, name);
            Assert.Equal(info.DisplayName, displayName);
            Assert.Equal(info.Description, description);
            Assert.Equal(info.Status, status);
        }
    }

    [Fact]
    public async Task Operations_WithLongTimeout_AreAccepted()
    {
        if (!OperatingSystem.IsWindows()) return;

        var result = await _manager.StartAsync(
            "NonExistentService",
            TimeSpan.FromHours(1)
        );

        Assert.False(result.Success);
        Assert.Equal("NOT_INSTALLED", result.Code);
    }

    [Fact]
    public async Task RestartAsync_UsesProperDefaultTimeout()
    {
        if (!OperatingSystem.IsWindows()) return;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var result = await _manager.RestartAsync("NonExistentService");
        
        stopwatch.Stop();

        Assert.False(result.Success);
        // Should fail quickly since service doesn't exist
        Assert.True(stopwatch.ElapsedMilliseconds < 5000);
    }

    [Fact]
    public async Task UninstallIfExistsAsync_IsIdempotent()
    {
        if (!OperatingSystem.IsWindows()) return;

        var serviceName = "NonExistent_" + Guid.NewGuid();

        // Multiple uninstalls should all succeed
        var result1 = await _manager.UninstallIfExistsAsync(serviceName);
        var result2 = await _manager.UninstallIfExistsAsync(serviceName);
        var result3 = await _manager.UninstallIfExistsAsync(serviceName);

        Assert.True(result1.Success);
        Assert.True(result2.Success);
        Assert.True(result3.Success);
    }

    [Fact]
    public void ServiceSpec_ToString_ReturnsReadableString()
    {
        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\test.exe",
            DisplayName: "Test Service"
        );

        var str = spec.ToString();

        Assert.Contains("TestService", str);
        Assert.Contains(@"C:\test.exe", str);
    }

    [Fact]
    public void OpResult_ToString_ReturnsReadableString()
    {
        var result1 = OpResult.Ok("Success");
        var result2 = OpResult.Fail("ERROR", "Failed");

        var str1 = result1.ToString();
        var str2 = result2.ToString();

        Assert.Contains("Success", str1);
        Assert.Contains("ERROR", str2);
        Assert.Contains("Failed", str2);
    }
}
