namespace peremunoz.WindowsServices;

/// <summary>
/// Represents the operational state of a Windows service.
/// </summary>
public enum ServiceStatus
{
    /// <summary>
    /// The service state is unknown or could not be determined.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The service is not installed on the system.
    /// </summary>
    NotInstalled,

    /// <summary>
    /// The service is stopped and not running.
    /// </summary>
    Stopped,

    /// <summary>
    /// The service is in the process of starting.
    /// </summary>
    StartPending,

    /// <summary>
    /// The service is in the process of stopping.
    /// </summary>
    StopPending,

    /// <summary>
    /// The service is running and operational.
    /// </summary>
    Running,

    /// <summary>
    /// The service is in the process of resuming from a paused state.
    /// </summary>
    PausePending,

    /// <summary>
    /// The service is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The service is in the process of continuing after being paused.
    /// </summary>
    ContinuePending
}
