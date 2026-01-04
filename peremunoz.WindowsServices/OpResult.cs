namespace peremunoz.WindowsServices;

/// <summary>
/// Represents the result of a service management operation.
/// </summary>
/// <param name="Success">Indicates whether the operation completed successfully.</param>
/// <param name="Code">A short code identifying the result status (e.g., "OK", "TIMEOUT", "WIN32_ERROR").</param>
/// <param name="Message">A human-readable message describing the operation result.</param>
/// <param name="Exception">The exception that caused the operation to fail, if applicable.</param>
public sealed record OpResult(bool Success, string Code, string Message, Exception? Exception = null)
{
    /// <summary>
    /// Creates a successful operation result.
    /// </summary>
    /// <param name="message">An optional message describing the successful operation. Defaults to "OK".</param>
    /// <returns>An <see cref="OpResult"/> indicating success.</returns>
    public static OpResult Ok(string message = "OK") => new(true, "OK", message);

    /// <summary>
    /// Creates a failed operation result.
    /// </summary>
    /// <param name="code">A code identifying the type of failure.</param>
    /// <param name="message">A message describing why the operation failed.</param>
    /// <param name="ex">The exception that caused the failure, if applicable.</param>
    /// <returns>An <see cref="OpResult"/> indicating failure.</returns>
    public static OpResult Fail(string code, string message, Exception? ex = null) => new(false, code, message, ex);
}
