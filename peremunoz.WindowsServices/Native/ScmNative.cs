using System.ComponentModel;
using System.Runtime.InteropServices;

namespace peremunoz.WindowsServices.Native;

internal static class ScmNative
{
    // Access rights
    internal const uint SC_MANAGER_CONNECT = 0x0001;
    internal const uint SC_MANAGER_CREATE_SERVICE = 0x0002;

    internal const uint SERVICE_QUERY_STATUS = 0x0004;
    internal const uint SERVICE_START = 0x0010;
    internal const uint SERVICE_STOP = 0x0020;
    internal const uint SERVICE_CHANGE_CONFIG = 0x0002;
    internal const uint DELETE = 0x00010000;

    internal const uint SERVICE_ALL_ACCESS = 0xF01FF;

    // Service types
    internal const uint SERVICE_WIN32_OWN_PROCESS = 0x00000010;

    // Start types (match ServiceStartMode values)
    internal const uint SERVICE_AUTO_START = 0x00000002;
    internal const uint SERVICE_DEMAND_START = 0x00000003;
    internal const uint SERVICE_DISABLED = 0x00000004;

    // Error control
    internal const uint SERVICE_ERROR_NORMAL = 0x00000001;

    // ChangeServiceConfig2 info levels
    internal const uint SERVICE_CONFIG_DESCRIPTION = 1;
    internal const uint SERVICE_CONFIG_DELAYED_AUTO_START_INFO = 3;

    // Common Win32 errors we may want to treat specially
    internal const int ERROR_SERVICE_DOES_NOT_EXIST = 1060;
    internal const int ERROR_SERVICE_MARKED_FOR_DELETE = 1072;

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern SafeScHandle OpenSCManager(
        string? lpMachineName,
        string? lpDatabaseName,
        uint dwDesiredAccess);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern SafeScHandle CreateService(
        SafeScHandle hSCManager,
        string lpServiceName,
        string lpDisplayName,
        uint dwDesiredAccess,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        string lpBinaryPathName,
        string? lpLoadOrderGroup,
        IntPtr lpdwTagId,
        string? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern SafeScHandle OpenService(
        SafeScHandle hSCManager,
        string lpServiceName,
        uint dwDesiredAccess);

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool ChangeServiceConfig(
        SafeScHandle hService,
        uint dwServiceType,
        uint dwStartType,
        uint dwErrorControl,
        string? lpBinaryPathName,
        string? lpLoadOrderGroup,
        IntPtr lpdwTagId,
        string? lpDependencies,
        string? lpServiceStartName,
        string? lpPassword,
        string? lpDisplayName);

    [DllImport("advapi32.dll", SetLastError = true)]
    internal static extern bool ChangeServiceConfig2(
        SafeScHandle hService,
        uint dwInfoLevel,
        IntPtr lpInfo);

    [DllImport("advapi32.dll", SetLastError = true)]
    internal static extern bool DeleteService(SafeScHandle hService);

    [DllImport("advapi32.dll", SetLastError = true)]
    internal static extern bool CloseServiceHandle(IntPtr hSCObject);

    internal static void ThrowLastWin32(string operation)
        => throw new Win32Exception(Marshal.GetLastWin32Error(), operation);
}
