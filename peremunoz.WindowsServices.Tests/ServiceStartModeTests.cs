namespace peremunoz.WindowsServices.Tests;

public class ServiceStartModeTests
{
    [Fact]
    public void EnumValues_MatchWindowsConstants()
    {
        // These values must match Windows SERVICE_START_TYPE constants
        Assert.Equal(2, (int)ServiceStartMode.Automatic);
        Assert.Equal(3, (int)ServiceStartMode.Manual);
        Assert.Equal(4, (int)ServiceStartMode.Disabled);
    }

    [Fact]
    public void AllEnumValues_CanBeParsed()
    {
        Assert.True(Enum.TryParse<ServiceStartMode>("Automatic", out var automatic));
        Assert.Equal(ServiceStartMode.Automatic, automatic);

        Assert.True(Enum.TryParse<ServiceStartMode>("Manual", out var manual));
        Assert.Equal(ServiceStartMode.Manual, manual);

        Assert.True(Enum.TryParse<ServiceStartMode>("Disabled", out var disabled));
        Assert.Equal(ServiceStartMode.Disabled, disabled);
    }

    [Fact]
    public void ToString_ReturnsEnumName()
    {
        Assert.Equal("Automatic", ServiceStartMode.Automatic.ToString());
        Assert.Equal("Manual", ServiceStartMode.Manual.ToString());
        Assert.Equal("Disabled", ServiceStartMode.Disabled.ToString());
    }
}
