namespace ScreenExposure.Core;

public sealed class ToneCurve
{
    private readonly CurvePoint[] points;

    public static ToneCurve Linear { get; } = new(new[]
    {
        new CurvePoint(0.0, 0.0),
        new CurvePoint(1.0, 1.0)
    });

    public ToneCurve(IEnumerable<CurvePoint> points)
    {
        ArgumentNullException.ThrowIfNull(points);

        this.points = points.OrderBy(point => point.X).ToArray();
        if (this.points.Length < 2)
        {
            throw new ArgumentException("A curve needs at least two control points.", nameof(points));
        }

        if (this.points[0].X != 0.0 || this.points[^1].X != 1.0)
        {
            throw new ArgumentException("A curve must include endpoints at X=0 and X=1.", nameof(points));
        }

        for (var index = 0; index < this.points.Length; index++)
        {
            var point = this.points[index];
            if (point.X < 0.0 || point.X > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(points), "Curve point X values must be between 0 and 1.");
            }

            if (index > 0 && point.X <= this.points[index - 1].X)
            {
                throw new ArgumentException("Curve point X values must be unique.", nameof(points));
            }
        }
    }

    public IReadOnlyList<CurvePoint> Points => points;

    public double Evaluate(double input)
    {
        var x = Clamp01(input);
        if (x <= points[0].X)
        {
            return Clamp01(points[0].Y);
        }

        for (var index = 1; index < points.Length; index++)
        {
            var right = points[index];
            if (x > right.X)
            {
                continue;
            }

            var left = points[index - 1];
            var span = right.X - left.X;
            var amount = span <= 0.0 ? 0.0 : (x - left.X) / span;
            return Clamp01(left.Y + ((right.Y - left.Y) * amount));
        }

        return Clamp01(points[^1].Y);
    }

    internal static double Clamp01(double value)
    {
        if (double.IsNaN(value))
        {
            return 0.0;
        }

        return Math.Clamp(value, 0.0, 1.0);
    }
}
