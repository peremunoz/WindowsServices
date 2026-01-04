namespace peremunoz.WindowsServices.Tests;

public class ServiceInfoTests
{
    [Fact]
    public void Constructor_AllParameters_CreatesInstance()
    {
        var info = new ServiceInfo(
            Name: "TestService",
            DisplayName: "Test Service",
            Description: "A test service",
            Status: ServiceStatus.Running
        );

        Assert.Equal("TestService", info.Name);
        Assert.Equal("Test Service", info.DisplayName);
        Assert.Equal("A test service", info.Description);
        Assert.Equal(ServiceStatus.Running, info.Status);
    }

    [Fact]
    public void Constructor_NullDescription_IsAllowed()
    {
        var info = new ServiceInfo(
            Name: "TestService",
            DisplayName: "Test Service",
            Description: null,
            Status: ServiceStatus.Stopped
        );

        Assert.Equal("TestService", info.Name);
        Assert.Null(info.Description);
    }

    [Fact]
    public void Record_Equality_WorksCorrectly()
    {
        var info1 = new ServiceInfo("Test", "Display", null, ServiceStatus.Running);
        var info2 = new ServiceInfo("Test", "Display", null, ServiceStatus.Running);
        var info3 = new ServiceInfo("Test", "Display", null, ServiceStatus.Stopped);

        Assert.Equal(info1, info2);
        Assert.NotEqual(info1, info3);
    }

    [Fact]
    public void With_Modification_CreatesNewInstance()
    {
        var original = new ServiceInfo("Test", "Display", null, ServiceStatus.Stopped);
        var modified = original with { Status = ServiceStatus.Running };

        Assert.Equal(ServiceStatus.Stopped, original.Status);
        Assert.Equal(ServiceStatus.Running, modified.Status);
        Assert.Equal(original.Name, modified.Name);
    }
}
