namespace peremunoz.WindowsServices;

/// <summary>
/// Contains information about a Windows service.
/// </summary>
/// <param name="Name">The unique name of the service used by the Service Control Manager.</param>
/// <param name="DisplayName">The friendly display name shown in the Services management console.</param>
/// <param name="Description">The description of the service's purpose and functionality, if available.</param>
/// <param name="Status">The current operational state of the service.</param>
public sealed record ServiceInfo(
    string Name,
    string DisplayName,
    string? Description,
    ServiceStatus Status
);
