using System.Runtime.InteropServices;
using ScreenExposure.Core;

namespace ScreenExposure.App;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct NativeGammaRamp
{
    public fixed ushort Red[GammaRamp.DefaultEntryCount];
    public fixed ushort Green[GammaRamp.DefaultEntryCount];
    public fixed ushort Blue[GammaRamp.DefaultEntryCount];

    public static NativeGammaRamp FromCore(GammaRamp ramp)
    {
        if (ramp.Red.Length != GammaRamp.DefaultEntryCount
            || ramp.Green.Length != GammaRamp.DefaultEntryCount
            || ramp.Blue.Length != GammaRamp.DefaultEntryCount)
        {
            throw new ArgumentException("Windows gamma ramp must contain 256 entries per channel.", nameof(ramp));
        }

        var native = new NativeGammaRamp();
        for (var index = 0; index < GammaRamp.DefaultEntryCount; index++)
        {
            native.Red[index] = ramp.Red[index];
            native.Green[index] = ramp.Green[index];
            native.Blue[index] = ramp.Blue[index];
        }

        return native;
    }
}
