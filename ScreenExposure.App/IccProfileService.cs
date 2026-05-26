using System.ComponentModel;
using System.IO;
using ScreenExposure.Core;

namespace ScreenExposure.App;

internal static class IccProfileService
{
    private static readonly string ColorProfileDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System),
        "spool",
        "drivers",
        "color");

    public static string ExportVcgtProfile(GammaRamp ramp, string profileName)
    {
        var sourcePath = Path.Combine(ColorProfileDirectory, "sRGB Color Space Profile.icm");
        if (!File.Exists(sourcePath))
        {
            sourcePath = Path.Combine(ColorProfileDirectory, "sRGB IEC61966-2.1.icm");
        }

        if (!File.Exists(sourcePath))
        {
            throw new FileNotFoundException("Could not find the Windows sRGB ICC profile to use as a base.", sourcePath);
        }

        var profile = File.ReadAllBytes(sourcePath);
        var output = IccProfileBuilder.WithVcgtTag(profile, ramp);
        Directory.CreateDirectory(ColorProfileDirectory);

        var safeName = string.Concat(profileName.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        var outputPath = Path.Combine(ColorProfileDirectory, $"{safeName}.icc");
        File.WriteAllBytes(outputPath, output);
        return outputPath;
    }

    public static void InstallAndAssociate(string profilePath, string deviceName)
    {
        if (!File.Exists(profilePath))
        {
            throw new FileNotFoundException("ICC profile file does not exist.", profilePath);
        }

        if (!NativeMethods.InstallColorProfileW(null, profilePath))
        {
            throw new Win32Exception($"InstallColorProfile failed for {profilePath}.");
        }

        if (!NativeMethods.AssociateColorProfileWithDeviceW(null, profilePath, deviceName))
        {
            throw new Win32Exception($"AssociateColorProfileWithDevice failed for {deviceName}.");
        }

        var profileNameLength = (uint)profilePath.Length;
        if (!NativeMethods.WcsSetDefaultColorProfile(
                NativeMethods.WcsProfileManagementScopeCurrentUser,
                deviceName,
                NativeMethods.ColorProfileTypeIcc,
                NativeMethods.ColorProfileSubtypeRgbDisplay,
                0,
                profileNameLength,
                profilePath))
        {
            throw new Win32Exception($"WcsSetDefaultColorProfile failed for {deviceName}.");
        }
    }
}
