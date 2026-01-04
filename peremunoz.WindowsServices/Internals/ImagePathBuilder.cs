namespace peremunoz.WindowsServices.Internals;

internal static class ImagePathBuilder
{
    public static string Build(string exePath, string? args)
    {
        Guard.NotNullOrWhiteSpace(exePath, nameof(exePath));

        var quotedExe = exePath.Contains(' ') ? $"\"{exePath}\"" : exePath;

        if (string.IsNullOrWhiteSpace(args))
            return quotedExe;

        return $"{quotedExe} {args}".Trim();
    }
}
