namespace peremunoz.WindowsServices;

public enum ServiceStatus
{
    Unknown = 0,
    NotInstalled,
    Stopped,
    StartPending,
    StopPending,
    Running,
    PausePending,
    Paused,
    ContinuePending
}
