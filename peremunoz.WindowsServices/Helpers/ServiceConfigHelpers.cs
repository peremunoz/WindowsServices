using System.Runtime.InteropServices;
using peremunoz.WindowsServices.Native;

namespace peremunoz.WindowsServices.Helpers;

internal static class ServiceConfigHelpers
{
    public static void ApplyDescription(SafeScHandle service, string? description)
    {
        var desc = new SERVICE_DESCRIPTION
        {
            // null clears description, which is acceptable
            lpDescription = description
        };

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(Marshal.SizeOf<SERVICE_DESCRIPTION>());
            Marshal.StructureToPtr(desc, ptr, fDeleteOld: false);

            if (!ScmNative.ChangeServiceConfig2(
                    service,
                    ScmNative.SERVICE_CONFIG_DESCRIPTION,
                    ptr))
            {
                ScmNative.ThrowLastWin32("ChangeServiceConfig2(DESCRIPTION)");
            }
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
        }
    }

    public static void ApplyDelayedAutoStart(SafeScHandle service, bool delayed)
    {
        var info = new SERVICE_DELAYED_AUTO_START_INFO
        {
            fDelayedAutostart = delayed
        };

        var ptr = IntPtr.Zero;
        try
        {
            ptr = Marshal.AllocHGlobal(Marshal.SizeOf<SERVICE_DELAYED_AUTO_START_INFO>());
            Marshal.StructureToPtr(info, ptr, fDeleteOld: false);

            if (!ScmNative.ChangeServiceConfig2(
                    service,
                    ScmNative.SERVICE_CONFIG_DELAYED_AUTO_START_INFO,
                    ptr))
            {
                ScmNative.ThrowLastWin32("ChangeServiceConfig2(DELAYED_AUTO_START)");
            }
        }
        finally
        {
            if (ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(ptr);
        }
    }
}
