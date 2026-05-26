using ScreenExposure.Core;

namespace ScreenExposure.Tests;

public sealed class GammaRampTests
{
    [Fact]
    public void Generate_Creates256EntriesPerChannel()
    {
        var ramp = GammaRamp.Generate(ColorAdjustment.Default);

        Assert.Equal(256, ramp.Red.Length);
        Assert.Equal(256, ramp.Green.Length);
        Assert.Equal(256, ramp.Blue.Length);
    }

    [Fact]
    public void Generate_DefaultRampStartsAtZeroAndEndsAtMax()
    {
        var ramp = GammaRamp.Generate(ColorAdjustment.Default);

        Assert.Equal(0, ramp.Red[0]);
        Assert.Equal(65535, ramp.Red[^1]);
        Assert.Equal(65535, ramp.Green[^1]);
        Assert.Equal(65535, ramp.Blue[^1]);
    }

    [Fact]
    public void Generate_DarkExposureLowersMidpoint()
    {
        var ramp = GammaRamp.Generate(ColorAdjustment.Default with { ExposureStops = -1.0 });

        Assert.InRange(ramp.Red[128], 16300, 16600);
        Assert.Equal(ramp.Red[128], ramp.Green[128]);
        Assert.Equal(ramp.Red[128], ramp.Blue[128]);
    }

    [Fact]
    public void Generate_NormalizesDescendingPresetCurvesForWindowsDrivers()
    {
        var adjustment = ColorAdjustment.Default with
        {
            MasterCurve = new ToneCurve(new[]
            {
                new CurvePoint(0.0, 0.0),
                new CurvePoint(0.5, 0.8),
                new CurvePoint(1.0, 0.6)
            })
        };

        var ramp = GammaRamp.Generate(adjustment);

        Assert.True(IsMonotonic(ramp.Red));
        Assert.True(IsMonotonic(ramp.Green));
        Assert.True(IsMonotonic(ramp.Blue));
    }

    private static bool IsMonotonic(ushort[] values)
    {
        for (var index = 1; index < values.Length; index++)
        {
            if (values[index] < values[index - 1])
            {
                return false;
            }
        }

        return true;
    }
}
