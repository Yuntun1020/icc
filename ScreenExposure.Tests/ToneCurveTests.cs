using ScreenExposure.Core;

namespace ScreenExposure.Tests;

public sealed class ToneCurveTests
{
    [Fact]
    public void Evaluate_InterpolatesBetweenControlPoints()
    {
        var curve = new ToneCurve(new[]
        {
            new CurvePoint(0.0, 0.0),
            new CurvePoint(0.5, 0.25),
            new CurvePoint(1.0, 1.0)
        });

        Assert.Equal(0.125, curve.Evaluate(0.25), precision: 6);
        Assert.Equal(0.625, curve.Evaluate(0.75), precision: 6);
    }

    [Fact]
    public void Evaluate_ClampsInputAndOutput()
    {
        var curve = new ToneCurve(new[]
        {
            new CurvePoint(0.0, -0.2),
            new CurvePoint(1.0, 1.2)
        });

        Assert.Equal(0.0, curve.Evaluate(-1.0), precision: 6);
        Assert.Equal(1.0, curve.Evaluate(2.0), precision: 6);
    }

    [Fact]
    public void Constructor_SortsPointsAndRequiresEndpoints()
    {
        var curve = new ToneCurve(new[]
        {
            new CurvePoint(1.0, 1.0),
            new CurvePoint(0.0, 0.0),
            new CurvePoint(0.75, 0.5)
        });

        Assert.Equal(0.25, curve.Evaluate(0.375), precision: 6);
    }
}
