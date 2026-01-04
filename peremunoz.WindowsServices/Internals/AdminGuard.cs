using System.Security.Principal;

namespace peremunoz.WindowsServices.Internals;

internal static class AdminGuard
{
    public static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}
