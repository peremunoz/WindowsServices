using Microsoft.Win32.SafeHandles;

namespace peremunoz.WindowsServices.Native;

internal sealed class SafeScHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeScHandle() : base(ownsHandle: true) { }

    protected override bool ReleaseHandle() => ScmNative.CloseServiceHandle(handle);
}
