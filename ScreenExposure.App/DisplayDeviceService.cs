using System.Windows.Forms;

namespace ScreenExposure.App;

internal static class DisplayDeviceService
{
    public static IReadOnlyList<DisplayDevice> GetDisplays()
    {
        return Screen.AllScreens
            .Select(screen => new DisplayDevice(screen.DeviceName, screen.Primary ? $"{screen.DeviceName} (Primary)" : screen.DeviceName))
            .ToArray();
    }
}
