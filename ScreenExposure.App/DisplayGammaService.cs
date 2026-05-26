using System.ComponentModel;
using ScreenExposure.Core;

namespace ScreenExposure.App;

internal sealed class DisplayGammaService : IDisposable
{
    private NativeGammaRamp? originalRamp;
    private bool disposed;

    public void Apply(GammaRamp ramp)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        using var context = DisplayDeviceContext.Acquire();
        originalRamp ??= context.GetGammaRamp();
        var nativeRamp = NativeGammaRamp.FromCore(ramp);
        context.SetGammaRamp(nativeRamp);
    }

    public void Restore()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (originalRamp is not { } ramp)
        {
            return;
        }

        using var context = DisplayDeviceContext.Acquire();
        context.SetGammaRamp(ramp);
        originalRamp = null;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        if (originalRamp is not null)
        {
            try
            {
                Restore();
            }
            catch (Win32Exception)
            {
                // Best-effort restore while the process is shutting down.
            }
        }

        disposed = true;
    }

    private sealed class DisplayDeviceContext : IDisposable
    {
        private readonly IntPtr hdc;

        private DisplayDeviceContext(IntPtr hdc)
        {
            this.hdc = hdc;
        }

        public static DisplayDeviceContext Acquire()
        {
            var hdc = NativeMethods.GetDC(IntPtr.Zero);
            if (hdc == IntPtr.Zero)
            {
                throw new Win32Exception("GetDC failed.");
            }

            return new DisplayDeviceContext(hdc);
        }

        public NativeGammaRamp GetGammaRamp()
        {
            var ramp = new NativeGammaRamp();
            if (!NativeMethods.GetDeviceGammaRamp(hdc, ref ramp))
            {
                throw new Win32Exception("GetDeviceGammaRamp failed.");
            }

            return ramp;
        }

        public void SetGammaRamp(NativeGammaRamp ramp)
        {
            if (!NativeMethods.SetDeviceGammaRamp(hdc, ref ramp))
            {
                throw new Win32Exception("SetDeviceGammaRamp failed. HDR, driver color controls, or protected fullscreen output may block it.");
            }
        }

        public void Dispose()
        {
            _ = NativeMethods.ReleaseDC(IntPtr.Zero, hdc);
        }
    }
}
