using peremunoz.WindowsServices.Internals;

namespace peremunoz.WindowsServices.Tests;

public class GuardTests
{
    [Fact]
    public void NotNullOrWhiteSpace_ValidString_DoesNotThrow()
    {
        var exception = Record.Exception(() => Guard.NotNullOrWhiteSpace("valid", "param"));
        Assert.Null(exception);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void NotNullOrWhiteSpace_InvalidString_ThrowsArgumentException(string? value)
    {
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrWhiteSpace(value!, "testParam"));
        Assert.Contains("Value cannot be null/empty", exception.Message);
        Assert.Equal("testParam", exception.ParamName);
    }

    [Fact]
    public void EnsureWindows_OnWindows_DoesNotThrow()
    {
        if (!OperatingSystem.IsWindows())
        {
            // Skip test on non-Windows platforms
            return;
        }

        var exception = Record.Exception(() => Guard.EnsureWindows());
        Assert.Null(exception);
    }
}
