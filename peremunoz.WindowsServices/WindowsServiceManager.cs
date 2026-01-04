using peremunoz.WindowsServices.Helpers;
using peremunoz.WindowsServices.Internals;
using peremunoz.WindowsServices.Native;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using TimeoutException = System.ServiceProcess.TimeoutException;

namespace peremunoz.WindowsServices;

/// <summary>
/// Provides Windows-specific implementation of service management operations.
/// </summary>
/// <remarks>
/// This class is only supported on Windows platforms and uses the Windows Service Control Manager (SCM) APIs.
/// Most operations require administrator privileges to succeed.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class WindowsServiceManager : IServiceManager
{
    private static readonly TimeSpan DefaultStartStopTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan DefaultRestartTimeout = TimeSpan.FromSeconds(60);

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ServiceStatus> GetStatusAsync(string name, CancellationToken ct = default)
    {
        var info = await TryGetAsync(name, ct).ConfigureAwait(false);
        return info?.Status ?? ServiceStatus.NotInstalled;
    }

    /// <inheritdoc />
    public Task<OpResult> InstallOrUpdateAsync(ServiceSpec spec, CancellationToken ct = default)
    {
        Guard.EnsureWindows();
        if (spec is null) throw new ArgumentNullException(nameof(spec));
        Guard.NotNullOrWhiteSpace(spec.Name, nameof(spec.Name));
        Guard.NotNullOrWhiteSpace(spec.ExePath, nameof(spec.ExePath));

        try
        {
            ct.ThrowIfCancellationRequested();

            var startType = spec.StartMode switch
            {
                ServiceStartMode.Automatic => ScmNative.SERVICE_AUTO_START,
                ServiceStartMode.Manual => ScmNative.SERVICE_DEMAND_START,
                ServiceStartMode.Disabled => ScmNative.SERVICE_DISABLED,
                _ => ScmNative.SERVICE_DEMAND_START
            };

            var displayName = string.IsNullOrWhiteSpace(spec.DisplayName) ? spec.Name : spec.DisplayName!;
            var imagePath = Internals.ImagePathBuilder.Build(spec.ExePath, spec.Arguments);

            using var scm = ScmNative.OpenSCManager(null, null, ScmNative.SC_MANAGER_CONNECT | ScmNative.SC_MANAGER_CREATE_SERVICE);
            if (scm.IsInvalid) ScmNative.ThrowLastWin32("OpenSCManager");

            // Try open existing first
            using var service = ScmNative.OpenService(scm, spec.Name, ScmNative.SERVICE_ALL_ACCESS);
            if (!service.IsInvalid)
            {
                // Update basic config
                if (!ScmNative.ChangeServiceConfig(
                        service,
                        dwServiceType: ScmNative.SERVICE_WIN32_OWN_PROCESS,
                        dwStartType: startType,
                        dwErrorControl: ScmNative.SERVICE_ERROR_NORMAL,
                        lpBinaryPathName: imagePath,
                        lpLoadOrderGroup: null,
                        lpdwTagId: IntPtr.Zero,
                        lpDependencies: null,
                        lpServiceStartName: null,
                        lpPassword: null,
                        lpDisplayName: displayName))
                {
                    ScmNative.ThrowLastWin32("ChangeServiceConfig");
                }

                ServiceConfigHelpers.ApplyDescription(service, spec.Description);
                ServiceConfigHelpers.ApplyDelayedAutoStart(service, spec.DelayedAutoStart);

                return Task.FromResult(OpResult.Ok($"Updated service '{spec.Name}'"));
            }

            // If open failed because it doesn't exist, create it
            var err = Marshal.GetLastWin32Error();
            if (err != ScmNative.ERROR_SERVICE_DOES_NOT_EXIST)
                return Task.FromResult(OpResult.Fail("WIN32_ERROR", new System.ComponentModel.Win32Exception(err).Message, new System.ComponentModel.Win32Exception(err)));

            using var created = ScmNative.CreateService(
                scm,
                lpServiceName: spec.Name,
                lpDisplayName: displayName,
                dwDesiredAccess: ScmNative.SERVICE_ALL_ACCESS,
                dwServiceType: ScmNative.SERVICE_WIN32_OWN_PROCESS,
                dwStartType: startType,
                dwErrorControl: ScmNative.SERVICE_ERROR_NORMAL,
                lpBinaryPathName: imagePath,
                lpLoadOrderGroup: null,
                lpdwTagId: IntPtr.Zero,
                lpDependencies: null,
                lpServiceStartName: null,
                lpPassword: null);

            if (created.IsInvalid) ScmNative.ThrowLastWin32("CreateService");

            ServiceConfigHelpers.ApplyDescription(created, spec.Description);
            ServiceConfigHelpers.ApplyDelayedAutoStart(created, spec.DelayedAutoStart);

            return Task.FromResult(OpResult.Ok($"Installed service '{spec.Name}'"));
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(OpResult.Fail("CANCELED", "Operation canceled."));
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return Task.FromResult(OpResult.Fail("WIN32_ERROR", ex.Message, ex));
        }
        catch (Exception ex)
        {
            return Task.FromResult(OpResult.Fail("ERROR", ex.Message, ex));
        }
    }

    /// <inheritdoc />
    public async Task<OpResult> UninstallIfExistsAsync(string name, CancellationToken ct = default)
    {
        Guard.EnsureWindows();
        Guard.NotNullOrWhiteSpace(name, nameof(name));

        try
        {
            ct.ThrowIfCancellationRequested();

            // Best-effort stop first (ignore if not installed)
            var status = await GetStatusAsync(name, ct).ConfigureAwait(false);
            if (status != ServiceStatus.NotInstalled && status != ServiceStatus.Stopped)
            {
                _ = await StopAsync(name, TimeSpan.FromSeconds(20), ct).ConfigureAwait(false);
            }

            using var scm = ScmNative.OpenSCManager(null, null, ScmNative.SC_MANAGER_CONNECT);
            if (scm.IsInvalid) ScmNative.ThrowLastWin32("OpenSCManager");

            using var service = ScmNative.OpenService(scm, name, ScmNative.DELETE);
            if (service.IsInvalid)
            {
                var err = Marshal.GetLastWin32Error();
                if (err == ScmNative.ERROR_SERVICE_DOES_NOT_EXIST)
                    return OpResult.Ok($"Service '{name}' not installed (no-op).");

                return OpResult.Fail("WIN32_ERROR", new System.ComponentModel.Win32Exception(err).Message, new System.ComponentModel.Win32Exception(err));
            }

            if (!ScmNative.DeleteService(service))
            {
                var err = Marshal.GetLastWin32Error();
                if (err == ScmNative.ERROR_SERVICE_MARKED_FOR_DELETE)
                    return OpResult.Ok($"Service '{name}' already marked for delete.");

                ScmNative.ThrowLastWin32("DeleteService");
            }

            return OpResult.Ok($"Uninstalled service '{name}'");
        }
        catch (OperationCanceledException)
        {
            return OpResult.Fail("CANCELED", "Operation canceled.");
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            return OpResult.Fail("WIN32_ERROR", ex.Message, ex);
        }
        catch (Exception ex)
        {
            return OpResult.Fail("ERROR", ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public Task<OpResult> StartAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)
        => ControlAsync(
            name,
            timeout ?? DefaultStartStopTimeout,
            action: sc => sc.Start(),
            desired: ServiceControllerStatus.Running,
            ct);

    /// <inheritdoc />
    public Task<OpResult> StopAsync(string name, TimeSpan? timeout = null, CancellationToken ct = default)
        => ControlAsync(
            name,
            timeout ?? DefaultStartStopTimeout,
            action: sc => sc.Stop(),
            desired: ServiceControllerStatus.Stopped,
            ct);

    /// <inheritdoc />
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
