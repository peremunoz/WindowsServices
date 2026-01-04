namespace peremunoz.WindowsServices;

public interface IServiceManager
{
    Task<ServiceInfo?> TryGetAsync(string name, CancellationToken ct = default);
    Task<ServiceStatus> GetStatusAsync(string name, CancellationToken ct = default);

    Task<OpResult> InstallOrUpdateAsync(ServiceSpec spec, CancellationToken ct = default);
    Task<OpResult> UninstallIfExistsAsync(string name, CancellationToken ct = default);

    Task<OpResult> StartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default);
    Task<OpResult> StopAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default);
    Task<OpResult> RestartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default);
}
