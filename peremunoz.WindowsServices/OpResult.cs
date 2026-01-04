namespace peremunoz.WindowsServices;

public sealed record OpResult(bool Success, string Code, string Message, Exception? Exception = null)
{
    public static OpResult Ok(string message = "OK") => new(true, "OK", message);
    public static OpResult Fail(string code, string message, Exception? ex = null) => new(false, code, message, ex);
}
