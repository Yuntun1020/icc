using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ScreenExposure.Core;
using InputKeyEventArgs = System.Windows.Input.KeyEventArgs;
using InputMouseEventArgs = System.Windows.Input.MouseEventArgs;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;
using InputCursors = System.Windows.Input.Cursors;
using WpfPoint = System.Windows.Point;

namespace ScreenExposure.App;

internal sealed class CurveEditor : FrameworkElement
{
    public static readonly DependencyProperty CurveProperty = DependencyProperty.Register(
        nameof(Curve),
        typeof(ToneCurve),
        typeof(CurveEditor),
        new FrameworkPropertyMetadata(ToneCurve.Linear, FrameworkPropertyMetadataOptions.AffectsRender, OnCurveChanged));

    private readonly List<CurvePoint> points = new()
    {
        new CurvePoint(0.0, 0.0),
        new CurvePoint(0.5, 0.5),
        new CurvePoint(1.0, 1.0)
    };

    private int? activePointIndex;

    public CurveEditor()
    {
        MinHeight = 260;
        Focusable = true;
        Cursor = InputCursors.Cross;
        Loaded += (_, _) => SyncPointsFromCurve();
    }

    public event EventHandler<ToneCurve>? CurveChanged;

    public ToneCurve Curve
    {
        get => (ToneCurve)GetValue(CurveProperty);
        set => SetValue(CurveProperty, value);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        Focus();
        CaptureMouse();

        var position = e.GetPosition(this);
        activePointIndex = FindNearestPoint(position);
        if (activePointIndex is null)
        {
            points.Add(ToCurvePoint(position));
            points.Sort((left, right) => left.X.CompareTo(right.X));
            activePointIndex = points.FindIndex(point => Math.Abs(point.X - ToCurvePoint(position).X) < 0.0001);
        }

        MoveActivePoint(position);
        e.Handled = true;
    }

    protected override void OnMouseMove(InputMouseEventArgs e)
    {
        if (activePointIndex is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        MoveActivePoint(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        activePointIndex = null;
        ReleaseMouseCapture();
        e.Handled = true;
    }

    protected override void OnKeyDown(InputKeyEventArgs e)
    {
        if (e.Key != Key.Delete || activePointIndex is null or 0 || activePointIndex == points.Count - 1)
        {
            base.OnKeyDown(e);
            return;
        }

        points.RemoveAt(activePointIndex.Value);
        activePointIndex = null;
        PublishCurve();
        e.Handled = true;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        var rect = new Rect(0, 0, ActualWidth, ActualHeight);
        drawingContext.DrawRectangle(new SolidColorBrush(MediaColor.FromRgb(18, 20, 23)), null, rect);

        DrawGrid(drawingContext, rect);
        DrawCurve(drawingContext);
        DrawPoints(drawingContext);
    }

    private static void OnCurveChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
    {
        var editor = (CurveEditor)dependencyObject;
        editor.SyncPointsFromCurve();
        editor.InvalidateVisual();
    }

    private void SyncPointsFromCurve()
    {
        points.Clear();
        points.AddRange(Curve.Points);
        if (points.Count == 2)
        {
            points.Insert(1, new CurvePoint(0.5, Curve.Evaluate(0.5)));
        }
    }

    private int? FindNearestPoint(WpfPoint position)
    {
        const double hitRadius = 12.0;
        for (var index = 0; index < points.Count; index++)
        {
            if ((ToScreenPoint(points[index]) - position).Length <= hitRadius)
            {
                return index;
            }
        }

        return null;
    }

    private void MoveActivePoint(WpfPoint position)
    {
        if (activePointIndex is not { } index)
        {
            return;
        }

        var point = ToCurvePoint(position);
        if (index == 0)
        {
            point = point with { X = 0.0 };
        }
        else if (index == points.Count - 1)
        {
            point = point with { X = 1.0 };
        }
        else
        {
            var min = points[index - 1].X + 0.01;
            var max = points[index + 1].X - 0.01;
            point = point with { X = Math.Clamp(point.X, min, max) };
        }

        points[index] = point;
        PublishCurve();
    }

    private void PublishCurve()
    {
        Curve = new ToneCurve(points);
        CurveChanged?.Invoke(this, Curve);
        InvalidateVisual();
    }

    private CurvePoint ToCurvePoint(WpfPoint point)
    {
        var width = Math.Max(1.0, ActualWidth);
        var height = Math.Max(1.0, ActualHeight);
        return new CurvePoint(
            Math.Clamp(point.X / width, 0.0, 1.0),
            Math.Clamp(1.0 - (point.Y / height), 0.0, 1.0));
    }

    private WpfPoint ToScreenPoint(CurvePoint point)
    {
        return new WpfPoint(point.X * ActualWidth, (1.0 - point.Y) * ActualHeight);
    }

    private void DrawGrid(DrawingContext drawingContext, Rect rect)
    {
        var gridPen = new MediaPen(new SolidColorBrush(MediaColor.FromRgb(47, 52, 58)), 1);
        var axisPen = new MediaPen(new SolidColorBrush(MediaColor.FromRgb(80, 88, 98)), 1.2);

        for (var i = 1; i < 4; i++)
        {
            var x = rect.Width * i / 4.0;
            var y = rect.Height * i / 4.0;
            drawingContext.DrawLine(gridPen, new WpfPoint(x, 0), new WpfPoint(x, rect.Height));
            drawingContext.DrawLine(gridPen, new WpfPoint(0, y), new WpfPoint(rect.Width, y));
        }

        drawingContext.DrawLine(axisPen, new WpfPoint(0, rect.Height), new WpfPoint(rect.Width, 0));
    }

    private void DrawCurve(DrawingContext drawingContext)
    {
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(ToScreenPoint(new CurvePoint(0.0, Curve.Evaluate(0.0))), false, false);
            for (var i = 1; i <= 96; i++)
            {
                var x = i / 96.0;
                context.LineTo(ToScreenPoint(new CurvePoint(x, Curve.Evaluate(x))), true, false);
            }
        }

        drawingContext.DrawGeometry(null, new MediaPen(new SolidColorBrush(MediaColor.FromRgb(93, 200, 164)), 2.5), geometry);
    }

    private void DrawPoints(DrawingContext drawingContext)
    {
        var fill = new SolidColorBrush(MediaColor.FromRgb(238, 243, 241));
        var border = new MediaPen(new SolidColorBrush(MediaColor.FromRgb(93, 200, 164)), 2);
        foreach (var point in points)
        {
            drawingContext.DrawEllipse(fill, border, ToScreenPoint(point), 5.5, 5.5);
        }
    }
}
