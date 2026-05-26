namespace ScreenExposure.Core;

public sealed record AdjustmentPreset(
    string Name,
    double ExposureStops,
    double Contrast,
    double Gamma,
    ToneCurve MasterCurve,
    ToneCurve RedCurve,
    ToneCurve GreenCurve,
    ToneCurve BlueCurve)
{
    public ColorAdjustment ToAdjustment()
    {
        return new ColorAdjustment(
            ExposureStops,
            Contrast,
            Gamma,
            MasterCurve,
            RedCurve,
            GreenCurve,
            BlueCurve);
    }
}

public static class AdjustmentPresets
{
    public static AdjustmentPreset Default { get; } = new(
        Name: "默认",
        ExposureStops: 0.0,
        Contrast: 0.0,
        Gamma: 1.0,
        MasterCurve: ToneCurve.Linear,
        RedCurve: ToneCurve.Linear,
        GreenCurve: ToneCurve.Linear,
        BlueCurve: ToneCurve.Linear);

    public static AdjustmentPreset OutdoorDim { get; } = new(
        Name: "室外压暗",
        ExposureStops: -1.1,
        Contrast: -0.08,
        Gamma: 0.78,
        MasterCurve: new ToneCurve(new[]
        {
            new CurvePoint(0.0, 0.0),
            new CurvePoint(0.38, 0.26),
            new CurvePoint(0.72, 0.53),
            new CurvePoint(1.0, 0.78)
        }),
        RedCurve: ToneCurve.Linear,
        GreenCurve: ToneCurve.Linear,
        BlueCurve: ToneCurve.Linear);

    public static AdjustmentPreset Night { get; } = new(
        Name: "夜间",
        ExposureStops: -1.5,
        Contrast: -0.15,
        Gamma: 0.7,
        MasterCurve: new ToneCurve(new[]
        {
            new CurvePoint(0.0, 0.0),
            new CurvePoint(0.5, 0.32),
            new CurvePoint(1.0, 0.64)
        }),
        RedCurve: ToneCurve.Linear,
        GreenCurve: ToneCurve.Linear,
        BlueCurve: new ToneCurve(new[]
        {
            new CurvePoint(0.0, 0.0),
            new CurvePoint(1.0, 0.82)
        }));
}
