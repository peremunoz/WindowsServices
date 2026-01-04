namespace peremunoz.WindowsServices.Tests;

public class ServiceSpecTests
{
    [Fact]
    public void Constructor_MinimalParameters_CreatesInstance()
    {
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
    public void Constructor_AllParameters_CreatesInstance()
    {
        var spec = new ServiceSpec(
            Name: "TestService",
            ExePath: @"C:\test.exe",
            Arguments: "--port 5000",
            DisplayName: "Test Service Display",
            Description: "A test service",
            StartMode: ServiceStartMode.Manual,
            DelayedAutoStart: true
        );

        Assert.Equal("TestService", spec.Name);
        Assert.Equal(@"C:\test.exe", spec.ExePath);
        Assert.Equal("--port 5000", spec.Arguments);
        Assert.Equal("Test Service Display", spec.DisplayName);
        Assert.Equal("A test service", spec.Description);
        Assert.Equal(ServiceStartMode.Manual, spec.StartMode);
        Assert.True(spec.DelayedAutoStart);
    }

    [Fact]
    public void Record_Equality_WorksCorrectly()
    {
        var spec1 = new ServiceSpec("Test", @"C:\test.exe");
        var spec2 = new ServiceSpec("Test", @"C:\test.exe");
        var spec3 = new ServiceSpec("Different", @"C:\test.exe");

        Assert.Equal(spec1, spec2);
        Assert.NotEqual(spec1, spec3);
    }

    [Fact]
    public void With_Modification_CreatesNewInstance()
    {
        var original = new ServiceSpec("Test", @"C:\test.exe");
        var modified = original with { Description = "New description" };

        Assert.Null(original.Description);
        Assert.Equal("New description", modified.Description);
        Assert.Equal(original.Name, modified.Name);
    }
}
