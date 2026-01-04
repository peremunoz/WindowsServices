namespace peremunoz.WindowsServices.Tests;

public class OpResultTests
{
    [Fact]
    public void Ok_DefaultMessage_CreatesSuccessResult()
    {
        var result = OpResult.Ok();

        Assert.True(result.Success);
        Assert.Equal("OK", result.Code);
        Assert.Equal("OK", result.Message);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Ok_CustomMessage_CreatesSuccessResult()
    {
        var result = OpResult.Ok("Service started successfully");

        Assert.True(result.Success);
        Assert.Equal("OK", result.Code);
        Assert.Equal("Service started successfully", result.Message);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Fail_MinimalParameters_CreatesFailureResult()
    {
        var result = OpResult.Fail("ERROR", "Something went wrong");

        Assert.False(result.Success);
        Assert.Equal("ERROR", result.Code);
        Assert.Equal("Something went wrong", result.Message);
        Assert.Null(result.Exception);
    }

    [Fact]
    public void Fail_WithException_CreatesFailureResult()
    {
        var exception = new InvalidOperationException("Test exception");
        var result = OpResult.Fail("ERROR", "Something went wrong", exception);

        Assert.False(result.Success);
        Assert.Equal("ERROR", result.Code);
        Assert.Equal("Something went wrong", result.Message);
        Assert.Same(exception, result.Exception);
    }

    [Fact]
    public void Record_Equality_WorksCorrectly()
    {
        var result1 = OpResult.Ok("Test");
        var result2 = OpResult.Ok("Test");
        var result3 = OpResult.Fail("ERROR", "Test");

        Assert.Equal(result1, result2);
        Assert.NotEqual(result1, result3);
    }

    [Fact]
    public void Record_Deconstruction_WorksCorrectly()
    {
        var result = OpResult.Fail("TIMEOUT", "Operation timed out", new TimeoutException());

        var (success, code, message, exception) = result;

        Assert.False(success);
        Assert.Equal("TIMEOUT", code);
        Assert.Equal("Operation timed out", message);
        Assert.IsType<TimeoutException>(exception);
    }
}
