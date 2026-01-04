namespace peremunoz.WindowsServices;

/// <summary>
/// Specifies the configuration for a Windows service to be installed or updated.
/// </summary>
/// <param name="Name">The unique name of the service used by the Service Control Manager.</param>
/// <param name="ExePath">The full path to the service executable.</param>
/// <param name="Arguments">Optional command-line arguments to pass to the service executable.</param>
/// <param name="DisplayName">The friendly display name shown in the Services management console. If <c>null</c>, the service name is used.</param>
/// <param name="Description">An optional description of the service's purpose and functionality.</param>
/// <param name="StartMode">The startup type for the service. Defaults to <see cref="ServiceStartMode.Automatic"/>.</param>
/// <param name="DelayedAutoStart">If <c>true</c> and <paramref name="StartMode"/> is <see cref="ServiceStartMode.Automatic"/>, the service starts shortly after boot to reduce boot time impact.</param>
public sealed record ServiceSpec(
    string Name,
    string ExePath,
    string? Arguments = null,
    string? DisplayName = null,
    string? Description = null,
    ServiceStartMode StartMode = ServiceStartMode.Automatic,
    bool DelayedAutoStart = false
);


/// <summary>
/// Specifies the startup behavior of a Windows service.
/// </summary>
public enum ServiceStartMode
{
    /// <summary>
    /// The service starts automatically when the system boots.
    /// </summary>
    Automatic = 2,

    /// <summary>
    /// The service must be started manually by a user or another service.
    /// </summary>
    Manual = 3,

    /// <summary>
    /// The service is disabled and cannot be started.
    /// </summary>
    Disabled = 4
}
