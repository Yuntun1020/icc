namespace ScreenExposure.Core;

public sealed record GammaRamp(ushort[] Red, ushort[] Green, ushort[] Blue)
{
    public const int DefaultEntryCount = 256;

    public static GammaRamp Generate(ColorAdjustment adjustment, int entryCount = DefaultEntryCount)
    {
        ArgumentNullException.ThrowIfNull(adjustment);
        if (entryCount < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(entryCount), "A gamma ramp needs at least two entries.");
        }

        var red = new ushort[entryCount];
        var green = new ushort[entryCount];
        var blue = new ushort[entryCount];

        for (var index = 0; index < entryCount; index++)
        {
            var input = index / (double)(entryCount - 1);
            red[index] = ToUInt16(adjustment.ApplyChannel(input, Channel.Red));
            green[index] = ToUInt16(adjustment.ApplyChannel(input, Channel.Green));
            blue[index] = ToUInt16(adjustment.ApplyChannel(input, Channel.Blue));
        }

        NormalizeMonotonic(red);
        NormalizeMonotonic(green);
        NormalizeMonotonic(blue);

        return new GammaRamp(red, green, blue);
    }

    private static void NormalizeMonotonic(ushort[] values)
    {
        for (var index = 1; index < values.Length; index++)
        {
            if (values[index] < values[index - 1])
            {
                values[index] = values[index - 1];
            }
        }
    }

    private static ushort ToUInt16(double value)
    {
        return (ushort)Math.Round(ToneCurve.Clamp01(value) * ushort.MaxValue, MidpointRounding.AwayFromZero);
    }
}
