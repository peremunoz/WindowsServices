namespace peremunoz.WindowsServices.Tests;

public class ServiceStatusTests
{
    [Fact]
    public void AllEnumValues_HaveExpectedValues()
    {
        Assert.Equal(0, (int)ServiceStatus.Unknown);
        Assert.Equal(1, (int)ServiceStatus.NotInstalled);
        Assert.Equal(2, (int)ServiceStatus.Stopped);
        Assert.Equal(3, (int)ServiceStatus.StartPending);
        Assert.Equal(4, (int)ServiceStatus.StopPending);
        Assert.Equal(5, (int)ServiceStatus.Running);
        Assert.Equal(6, (int)ServiceStatus.PausePending);
        Assert.Equal(7, (int)ServiceStatus.Paused);
        Assert.Equal(8, (int)ServiceStatus.ContinuePending);
    }

    [Fact]
    public void AllEnumValues_CanBeParsed()
    {
        Assert.True(Enum.TryParse<ServiceStatus>("Running", out var running));
        Assert.Equal(ServiceStatus.Running, running);

        Assert.True(Enum.TryParse<ServiceStatus>("Stopped", out var stopped));
        Assert.Equal(ServiceStatus.Stopped, stopped);

        Assert.True(Enum.TryParse<ServiceStatus>("NotInstalled", out var notInstalled));
        Assert.Equal(ServiceStatus.NotInstalled, notInstalled);
    }

    [Fact]
    public void ToString_ReturnsEnumName()
    {
        Assert.Equal("Running", ServiceStatus.Running.ToString());
        Assert.Equal("Stopped", ServiceStatus.Stopped.ToString());
        Assert.Equal("NotInstalled", ServiceStatus.NotInstalled.ToString());
    }
}
