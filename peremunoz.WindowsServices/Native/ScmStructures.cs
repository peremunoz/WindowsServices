using System.Runtime.InteropServices;

namespace peremunoz.WindowsServices.Native;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct SERVICE_DESCRIPTION
{
    public string? lpDescription;
}

[StructLayout(LayoutKind.Sequential)]
internal struct SERVICE_DELAYED_AUTO_START_INFO
{
    [MarshalAs(UnmanagedType.Bool)]
    public bool fDelayedAutostart;
}
