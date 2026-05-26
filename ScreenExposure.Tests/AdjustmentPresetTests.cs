using ScreenExposure.Core;

namespace ScreenExposure.Tests;

public sealed class AdjustmentPresetTests
{
    [Theory]
    [MemberData(nameof(Presets))]
    public void Preset_GeneratesValidWindowsGammaRamp(AdjustmentPreset preset)
    {
        var ramp = GammaRamp.Generate(preset.ToAdjustment());

        Assert.Equal(GammaRamp.DefaultEntryCount, ramp.Red.Length);
        Assert.Equal(GammaRamp.DefaultEntryCount, ramp.Green.Length);
        Assert.Equal(GammaRamp.DefaultEntryCount, ramp.Blue.Length);
    }

    [Theory]
    [MemberData(nameof(Presets))]
    public void Preset_KeepsWhitePointInWindowsDriverSafeRange(AdjustmentPreset preset)
    {
        var ramp = GammaRamp.Generate(preset.ToAdjustment());

        Assert.True(ramp.Red[^1] >= GammaRamp.WindowsDriverSafeWhitePoint);
        Assert.True(ramp.Green[^1] >= GammaRamp.WindowsDriverSafeWhitePoint);
        Assert.True(ramp.Blue[^1] >= GammaRamp.WindowsDriverSafeWhitePoint);
    }

    [Fact]
    public void OutdoorPreset_ResetsAllPerChannelCurvesToLinear()
    {
        Assert.Same(ToneCurve.Linear, AdjustmentPresets.OutdoorDim.RedCurve);
        Assert.Same(ToneCurve.Linear, AdjustmentPresets.OutdoorDim.GreenCurve);
        Assert.Same(ToneCurve.Linear, AdjustmentPresets.OutdoorDim.BlueCurve);
    }

    [Fact]
    public void NightPreset_OnlyChangesBlueChannel()
    {
        Assert.Same(ToneCurve.Linear, AdjustmentPresets.Night.RedCurve);
        Assert.Same(ToneCurve.Linear, AdjustmentPresets.Night.GreenCurve);
        Assert.NotSame(ToneCurve.Linear, AdjustmentPresets.Night.BlueCurve);
    }

    public static IEnumerable<object[]> Presets()
    {
        yield return new object[] { AdjustmentPresets.Default };
        yield return new object[] { AdjustmentPresets.OutdoorDim };
        yield return new object[] { AdjustmentPresets.Night };
    }
}
