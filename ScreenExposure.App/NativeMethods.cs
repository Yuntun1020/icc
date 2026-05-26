using System.Runtime.InteropServices;

namespace ScreenExposure.App;

internal static partial class NativeMethods
{
    internal const uint WcsProfileManagementScopeCurrentUser = 0;
    internal const uint ColorProfileTypeIcc = 0;
    internal const uint ColorProfileSubtypeRgbDisplay = 0;

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetDeviceGammaRamp(IntPtr hdc, ref NativeGammaRamp ramp);

    [LibraryImport("gdi32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetDeviceGammaRamp(IntPtr hdc, ref NativeGammaRamp ramp);

    [LibraryImport("user32.dll")]
    internal static partial IntPtr GetDC(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    internal static partial int ReleaseDC(IntPtr hWnd, IntPtr hdc);

    [LibraryImport("mscms.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool InstallColorProfileW(string? machineName, string profileName);

    [LibraryImport("mscms.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool AssociateColorProfileWithDeviceW(string? machineName, string profileName, string deviceName);

    [LibraryImport("mscms.dll", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool WcsSetDefaultColorProfile(
        uint scope,
        string? deviceName,
        uint profileType,
        uint profileSubtype,
        uint profileId,
        uint profileNameLength,
        string profileName);
}
