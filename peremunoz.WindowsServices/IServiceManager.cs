namespace peremunoz.WindowsServices;

/// <summary>
/// Defines operations for managing Windows services including installation, control, and status queries.
/// </summary>
public interface IServiceManager
{
    /// <summary>
    /// Attempts to retrieve information about a Windows service by name.
    /// </summary>
    /// <param name="name">The name of the service to query.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="ServiceInfo"/> object if the service exists and is accessible; otherwise, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// This is a non-throwing method. If the service does not exist or access is denied, it returns <c>null</c>.
    /// </remarks>
    Task<ServiceInfo?> TryGetAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Gets the current status of a Windows service.
    /// </summary>
    /// <param name="name">The name of the service to query.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="ServiceStatus"/> value representing the current state of the service.
    /// Returns <see cref="ServiceStatus.NotInstalled"/> if the service does not exist.
    /// </returns>
    Task<ServiceStatus> GetStatusAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Installs a new Windows service or updates an existing one with the specified configuration.
    /// </summary>
    /// <param name="spec">The service specification containing configuration details.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="OpResult"/> indicating success or failure of the operation.
    /// </returns>
    /// <remarks>
    /// If the service already exists, its configuration is updated. If it does not exist, a new service is created.
    /// This operation typically requires administrator privileges.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spec"/> is <c>null</c>.</exception>
    Task<OpResult> InstallOrUpdateAsync(ServiceSpec spec, CancellationToken ct = default);

    /// <summary>
    /// Uninstalls a Windows service if it exists.
    /// </summary>
    /// <param name="name">The name of the service to uninstall.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="OpResult"/> indicating success or failure of the operation.
    /// Returns success if the service does not exist (no-op).
    /// </returns>
    /// <remarks>
    /// If the service is running, it will be stopped before uninstallation.
    /// This operation typically requires administrator privileges.
    /// </remarks>
    Task<OpResult> UninstallIfExistsAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// Starts a Windows service and waits for it to reach the running state.
    /// </summary>
    /// <param name="name">The name of the service to start.</param>
    /// <param name="timeout">
    /// The maximum time to wait for the service to start. If <c>null</c>, a default timeout of 30 seconds is used.
    /// </param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="OpResult"/> indicating success or failure of the operation.
    /// </returns>
    /// <remarks>
    /// If the service is already running, the operation succeeds immediately without attempting to start it again.
    /// </remarks>
    Task<OpResult> StartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default);

    /// <summary>
    /// Stops a Windows service and waits for it to reach the stopped state.
    /// </summary>
    /// <param name="name">The name of the service to stop.</param>
    /// <param name="timeout">
    /// The maximum time to wait for the service to stop. If <c>null</c>, a default timeout of 30 seconds is used.
    /// </param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="OpResult"/> indicating success or failure of the operation.
    /// </returns>
    /// <remarks>
    /// If the service is already stopped, the operation succeeds immediately without attempting to stop it again.
    /// </remarks>
    Task<OpResult> StopAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default);

    /// <summary>
    /// Restarts a Windows service by stopping it and then starting it again.
    /// </summary>
    /// <param name="name">The name of the service to restart.</param>
    /// <param name="timeout">
    /// The maximum total time to wait for both stop and start operations. If <c>null</c>, a default timeout of 60 seconds is used.
    /// </param>
    /// <param name="ct">A cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="OpResult"/> indicating success or failure of the operation.
    /// </returns>
    /// <remarks>
    /// This method stops the service first, then starts it. If the service is already stopped, only the start operation is performed.
    /// </remarks>
    Task<OpResult> RestartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default);
}
