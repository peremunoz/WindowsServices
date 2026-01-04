namespace peremunoz.WindowsServices.Internals;

internal static class Guard
{
    [SupportedOSPlatform("windows")]
    public static void EnsureWindows()
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("peremunoz.WindowsServices supports Windows only.");
    }

    public static void NotNullOrWhiteSpace(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null/empty.", paramName);
    }
}
