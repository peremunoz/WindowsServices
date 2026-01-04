namespace peremunoz.WindowsServices;

public sealed record ServiceSpec(
    string Name,
    string ExePath,
    string? Arguments = null,
    string? DisplayName = null,
    string? Description = null,
    ServiceStartMode StartMode = ServiceStartMode.Automatic,
    bool DelayedAutoStart = false
);

public enum ServiceStartMode
{
    Automatic = 2,
    Manual = 3,
    Disabled = 4
}
