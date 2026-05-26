using ScreenExposure.Core;

namespace ScreenExposure.Tests;

public sealed class ColorAdjustmentTests
{
    [Fact]
    public void Apply_ExposureStopDarkensMidtonesByHalf()
    {
        var adjustment = ColorAdjustment.Default with { ExposureStops = -1.0 };

        var result = adjustment.ApplyChannel(0.5, Channel.Master);

        Assert.Equal(0.25, result, precision: 6);
    }

    [Fact]
    public void Apply_GammaBelowOneDarkensMidtones()
    {
        var adjustment = ColorAdjustment.Default with { Gamma = 0.5 };

        var result = adjustment.ApplyChannel(0.5, Channel.Master);

        Assert.InRange(result, 0.24, 0.26);
    }

    [Fact]
    public void Apply_ChannelCurveAffectsOnlyThatChannel()
    {
        var adjustment = ColorAdjustment.Default with
        {
            RedCurve = new ToneCurve(new[]
            {
                new CurvePoint(0.0, 0.0),
                new CurvePoint(1.0, 0.5)
            })
        };

        Assert.Equal(0.25, adjustment.ApplyChannel(0.5, Channel.Red), precision: 6);
        Assert.Equal(0.5, adjustment.ApplyChannel(0.5, Channel.Green), precision: 6);
        Assert.Equal(0.5, adjustment.ApplyChannel(0.5, Channel.Blue), precision: 6);
    }
}
