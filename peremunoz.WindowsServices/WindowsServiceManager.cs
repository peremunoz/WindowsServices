using System.ServiceProcess;
using System.ComponentModel;
using peremunoz.WindowsServices.Internals;
using TimeoutException = System.ServiceProcess.TimeoutException;

namespace peremunoz.WindowsServices;

[SupportedOSPlatform("windows")]
public sealed class WindowsServiceManager : IServiceManager
{
    private static readonly TimeSpan DefaultStartStopTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultRestartTimeout = TimeSpan.FromSeconds(60);

    public Task<ServiceInfo?> TryGetAsync(string name, CancellationToken ct = default)
    {
        Guard.EnsureWindows();
        Guard.NotNullOrWhiteSpace(name, nameof(name));

        try
        {
            using var sc = new ServiceController(name);

            // Force resolution. If not installed it typically throws InvalidOperationException.
            var status = MapStatus(sc.Status);

            // Description isn't available from ServiceController (we'll add in Milestone 2+).
            return Task.FromResult<ServiceInfo?>(new ServiceInfo(sc.ServiceName, sc.DisplayName, null, status));
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult<ServiceInfo?>(null);
        }
        catch (Win32Exception)
        {
            // Could be access/lookup issues; treat as not found for TryGet (non-throwing API).
            return Task.FromResult<ServiceInfo?>(null);
        }
    }

    public async Task<ServiceStatus> GetStatusAsync(string name, CancellationToken ct = default)
    {
        var info = await TryGetAsync(name, ct).ConfigureAwait(false);
        return info?.Status ?? ServiceStatus.NotInstalled;
    }

    public Task<OpResult> InstallOrUpdateAsync(ServiceSpec spec, CancellationToken ct = default)
        => Task.FromResult(OpResult.Fail("NOT_IMPLEMENTED", "Install/Update will be implemented in Milestone 2 (SCM APIs)."));

    public Task<OpResult> UninstallIfExistsAsync(string name, CancellationToken ct = default)
        => Task.FromResult(OpResult.Fail("NOT_IMPLEMENTED", "Uninstall will be implemented in Milestone 2 (SCM APIs)."));

    public Task<OpResult> StartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)
        => ControlAsync(
            name,
            timeout ?? DefaultStartStopTimeout,
            action: sc => sc.Start(),
            desired: ServiceControllerStatus.Running,
            ct);

    public Task<OpResult> StopAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)
        => ControlAsync(
            name,
            timeout ?? DefaultStartStopTimeout,
            action: sc => sc.Stop(),
            desired: ServiceControllerStatus.Stopped,
            ct);

    public async Task<OpResult> RestartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)
    {
        timeout ??= DefaultRestartTimeout;

        // If it's not installed, StopAsync returns NOT_INSTALLED.
        var stop = await StopAsync(name, timeout.Value, ct).ConfigureAwait(false);
        if (!stop.Success && stop.Code != "ALREADY_STOPPED") return stop;

        var start = await StartAsync(name, timeout.Value, ct).ConfigureAwait(false);
        return start;
    }

    private static async Task<OpResult> ControlAsync(
        string name,
        TimeSpan timeout,
        Action<ServiceController> action,
        ServiceControllerStatus desired,
        CancellationToken ct)
    {
        Guard.EnsureWindows();
        Guard.NotNullOrWhiteSpace(name, nameof(name));

        try
        {
            using var sc = new ServiceController(name);

            // Force resolution early.
            var current = sc.Status;

            // Idempotent-ish: avoid calling Start/Stop when already in desired state.
            if (current == desired)
            {
                var code = desired == ServiceControllerStatus.Stopped ? "ALREADY_STOPPED" : "ALREADY_RUNNING";
                return OpResult.Ok($"{name}: {code}");
            }

            action(sc);
            await WaitForStatusAsync(sc, desired, timeout, ct).ConfigureAwait(false);

            return OpResult.Ok($"{name}: {desired}");
        }
        catch (InvalidOperationException ex)
        {
            return OpResult.Fail("NOT_INSTALLED", $"Service '{name}' is not installed.", ex);
        }
        catch (Win32Exception ex)
        {
            // Access denied, service cannot accept control messages, etc.
            return OpResult.Fail("WIN32_ERROR", ex.Message, ex);
        }
        catch (TimeoutException ex)
        {
            return OpResult.Fail("TIMEOUT", ex.Message, ex);
        }
        catch (Exception ex)
        {
            return OpResult.Fail("ERROR", ex.Message, ex);
        }
    }

    private static async Task WaitForStatusAsync(ServiceController sc, ServiceControllerStatus desired, TimeSpan timeout, CancellationToken ct)
    {
        var start = DateTime.UtcNow;

        // Polling is cancellable. ServiceController.WaitForStatus is not.
        while (true)
        {
            ct.ThrowIfCancellationRequested();

            sc.Refresh();
            if (sc.Status == desired) return;

            if (DateTime.UtcNow - start > timeout)
                throw new TimeoutException($"Timed out waiting for '{sc.ServiceName}' to reach '{desired}'.");

            await Task.Delay(250, ct).ConfigureAwait(false);
        }
    }

    private static ServiceStatus MapStatus(ServiceControllerStatus s) => s switch
    {
        ServiceControllerStatus.Stopped => ServiceStatus.Stopped,
        ServiceControllerStatus.StartPending => ServiceStatus.StartPending,
        ServiceControllerStatus.StopPending => ServiceStatus.StopPending,
        ServiceControllerStatus.Running => ServiceStatus.Running,
        ServiceControllerStatus.ContinuePending => ServiceStatus.ContinuePending,
        ServiceControllerStatus.PausePending => ServiceStatus.PausePending,
        ServiceControllerStatus.Paused => ServiceStatus.Paused,
        _ => ServiceStatus.Unknown
    };
}
