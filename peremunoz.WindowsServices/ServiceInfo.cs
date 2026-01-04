namespace peremunoz.WindowsServices;

public sealed record ServiceInfo(
    string Name,
    string DisplayName,
    string? Description,
    ServiceStatus Status
);
