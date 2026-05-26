namespace ScreenExposure.Core;

public sealed record ColorAdjustment(
    double ExposureStops,
    double Contrast,
    double Gamma,
    ToneCurve MasterCurve,
    ToneCurve RedCurve,
    ToneCurve GreenCurve,
    ToneCurve BlueCurve)
{
    public static ColorAdjustment Default { get; } = new(
        ExposureStops: 0.0,
        Contrast: 0.0,
        Gamma: 1.0,
        MasterCurve: ToneCurve.Linear,
        RedCurve: ToneCurve.Linear,
        GreenCurve: ToneCurve.Linear,
        BlueCurve: ToneCurve.Linear);

    public double ApplyChannel(double input, Channel channel)
    {
        var value = ToneCurve.Clamp01(input);

        value *= Math.Pow(2.0, ExposureStops);

        var contrastFactor = Math.Max(0.0, 1.0 + Contrast);
        value = ((value - 0.5) * contrastFactor) + 0.5;
        value = ToneCurve.Clamp01(value);

        var gamma = Gamma <= 0.01 ? 0.01 : Gamma;
        value = Math.Pow(value, 1.0 / gamma);
        value = MasterCurve.Evaluate(value);

        return channel switch
        {
            Channel.Red => RedCurve.Evaluate(value),
            Channel.Green => GreenCurve.Evaluate(value),
            Channel.Blue => BlueCurve.Evaluate(value),
            _ => ToneCurve.Clamp01(value)
        };
    }
}
