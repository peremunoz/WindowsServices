using peremunoz.WindowsServices.Internals;

namespace peremunoz.WindowsServices.Tests;

public class ImagePathBuilderTests
{
    [Fact]
    public void Build_NoSpaces_NoArgs_ReturnsExe()
    {
        var result = ImagePathBuilder.Build(@"C:\svc\app.exe", null);
        Assert.Equal(@"C:\svc\app.exe", result);
    }

    [Fact]
    public void Build_Spaces_NoArgs_QuotesExe()
    {
        var result = ImagePathBuilder.Build(@"C:\Program Files\MySvc\app.exe", null);
        Assert.Equal("\"C:\\Program Files\\MySvc\\app.exe\"", result);
    }

    [Fact]
    public void Build_Spaces_WithArgs_QuotesExeAndAppendsArgs()
    {
        var result = ImagePathBuilder.Build(@"C:\Program Files\MySvc\app.exe", "--port 5000");
        Assert.Equal("\"C:\\Program Files\\MySvc\\app.exe\" --port 5000", result);
    }
}
