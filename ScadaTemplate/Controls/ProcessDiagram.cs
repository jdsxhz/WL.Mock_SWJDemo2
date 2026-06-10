using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScadaTemplate.Controls;

public sealed class ProcessDiagram : Control
{
    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(ProcessDiagram),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Valve1OpenProperty =
        DependencyProperty.Register(nameof(Valve1Open), typeof(bool), typeof(ProcessDiagram),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Valve2OpenProperty =
        DependencyProperty.Register(nameof(Valve2Open), typeof(bool), typeof(ProcessDiagram),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Pump1RunningProperty =
        DependencyProperty.Register(nameof(Pump1Running), typeof(bool), typeof(ProcessDiagram),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Pump2RunningProperty =
        DependencyProperty.Register(nameof(Pump2Running), typeof(bool), typeof(ProcessDiagram),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TankTemperature1Property =
        RegisterMetric(nameof(TankTemperature1), 14d);

    public static readonly DependencyProperty TankTemperature2Property =
        RegisterMetric(nameof(TankTemperature2), 14d);

    public static readonly DependencyProperty InletTemperatureProperty =
        RegisterMetric(nameof(InletTemperature), 14d);

    public static readonly DependencyProperty OutletTemperatureProperty =
        RegisterMetric(nameof(OutletTemperature), 14d);

    public static readonly DependencyProperty FilterLevel1Property =
        RegisterMetric(nameof(FilterLevel1), 70d);

    public static readonly DependencyProperty FilterLevel2Property =
        RegisterMetric(nameof(FilterLevel2), 65d);

    private readonly Typeface typeface = new("Microsoft YaHei UI");

    static ProcessDiagram()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ProcessDiagram), new FrameworkPropertyMetadata(typeof(ProcessDiagram)));
    }

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    public bool Valve1Open
    {
        get => (bool)GetValue(Valve1OpenProperty);
        set => SetValue(Valve1OpenProperty, value);
    }

    public bool Valve2Open
    {
        get => (bool)GetValue(Valve2OpenProperty);
        set => SetValue(Valve2OpenProperty, value);
    }

    public bool Pump1Running
    {
        get => (bool)GetValue(Pump1RunningProperty);
        set => SetValue(Pump1RunningProperty, value);
    }

    public bool Pump2Running
    {
        get => (bool)GetValue(Pump2RunningProperty);
        set => SetValue(Pump2RunningProperty, value);
    }

    public double TankTemperature1
    {
        get => (double)GetValue(TankTemperature1Property);
        set => SetValue(TankTemperature1Property, value);
    }

    public double TankTemperature2
    {
        get => (double)GetValue(TankTemperature2Property);
        set => SetValue(TankTemperature2Property, value);
    }

    public double InletTemperature
    {
        get => (double)GetValue(InletTemperatureProperty);
        set => SetValue(InletTemperatureProperty, value);
    }

    public double OutletTemperature
    {
        get => (double)GetValue(OutletTemperatureProperty);
        set => SetValue(OutletTemperatureProperty, value);
    }

    public double FilterLevel1
    {
        get => (double)GetValue(FilterLevel1Property);
        set => SetValue(FilterLevel1Property, value);
    }

    public double FilterLevel2
    {
        get => (double)GetValue(FilterLevel2Property);
        set => SetValue(FilterLevel2Property, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var sx = Math.Max(ActualWidth, 1) / 1260.0;
        var sy = Math.Max(ActualHeight, 1) / 720.0;
        dc.PushTransform(new ScaleTransform(sx, sy));

        var blue = IsRunning ? Colors.DodgerBlue : Color.FromRgb(42, 78, 103);
        var yellow = IsRunning ? Colors.Gold : Color.FromRgb(98, 86, 39);

        DrawPipe(dc, new Point(50, 150), new Point(570, 150), blue);
        DrawPipe(dc, new Point(50, 330), new Point(560, 330), blue);
        DrawPipe(dc, new Point(70, 560), new Point(1150, 560), blue);
        DrawPipe(dc, new Point(590, 220), new Point(900, 220), yellow);
        DrawPipe(dc, new Point(590, 430), new Point(900, 430), yellow);
        DrawPipe(dc, new Point(900, 220), new Point(900, 300), yellow);
        DrawPipe(dc, new Point(900, 430), new Point(900, 500), yellow);
        DrawPipe(dc, new Point(1050, 250), new Point(1050, 560), blue);

        DrawFilter(dc, 140, 95, FilterLevel1);
        DrawFilter(dc, 140, 275, FilterLevel2);
        DrawValve(dc, 385, 120, Valve1Open);
        DrawValve(dc, 385, 300, Valve2Open);
        DrawTank(dc, 590, 155, TankTemperature1, "中间罐1");
        DrawTank(dc, 590, 365, TankTemperature2, "中间罐2");
        DrawPump(dc, 930, 250, Pump1Running, "泵1");
        DrawPump(dc, 930, 500, Pump2Running, "泵2");
        DrawVerticalTank(dc, 120, 485, InletTemperature);
        DrawMeter(dc, 315, 485, OutletTemperature);
        DrawStateBadge(dc, 30, 20, IsRunning ? "流程运行" : "流程停止", IsRunning ? Color.FromRgb(65, 234, 154) : Color.FromRgb(84, 126, 155));

        dc.Pop();
    }

    private static DependencyProperty RegisterMetric(string name, double defaultValue)
    {
        return DependencyProperty.Register(name, typeof(double), typeof(ProcessDiagram),
            new FrameworkPropertyMetadata(defaultValue, FrameworkPropertyMetadataOptions.AffectsRender));
    }

    private static void DrawPipe(DrawingContext dc, Point start, Point end, Color color)
    {
        var pen = new Pen(new SolidColorBrush(color), 5) { StartLineCap = PenLineCap.Square, EndLineCap = PenLineCap.Square };
        dc.DrawLine(pen, start, end);
    }

    private void DrawFilter(DrawingContext dc, double x, double y, double level)
    {
        var cyan = new SolidColorBrush(Color.FromRgb(87, 244, 238));
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(15, 116, 148)), 3);
        dc.DrawRoundedRectangle(cyan, pen, new Rect(x, y, 170, 95), 12, 12);
        dc.DrawGeometry(new SolidColorBrush(Color.FromRgb(58, 206, 202)), pen, Geometry.Parse($"M{x},{y + 12} L{x + 35},{y + 36} L{x + 35},{y + 128} L{x},{y + 95} Z"));

        for (var row = 0; row < 3; row++)
        {
            for (var col = 0; col < 5; col++)
            {
                dc.DrawEllipse(null, pen, new Point(x + 48 + col * 30, y + 20 + row * 30), 10, 10);
            }
        }

        var gaugeHeight = Math.Clamp(level, 0, 100) / 100d * 120d;
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(70, 40, 80, 95)), null, new Rect(x - 30, y, 14, 120), 7, 7);
        dc.DrawRoundedRectangle(new LinearGradientBrush(Color.FromRgb(111, 255, 244), Color.FromRgb(21, 218, 103), 90), null, new Rect(x - 30, y + 120 - gaugeHeight, 14, gaugeHeight), 7, 7);
        DrawText(dc, $"{level:0}%", x - 45, y + 126, 14, Color.FromRgb(120, 244, 238), FontWeights.Bold);
    }

    private void DrawValve(DrawingContext dc, double x, double y, bool isOpen)
    {
        var body = new SolidColorBrush(isOpen ? Color.FromRgb(94, 230, 236) : Color.FromRgb(84, 113, 128));
        var top = new SolidColorBrush(isOpen ? Color.FromRgb(52, 220, 116) : Color.FromRgb(232, 91, 47));
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(22, 118, 145)), 3);
        dc.DrawRectangle(body, pen, new Rect(x, y + 34, 42, 24));
        dc.DrawRectangle(top, null, new Rect(x + 14, y, 22, 34));
        dc.DrawLine(new Pen(body, 5), new Point(x + 20, y + 34), new Point(x + 20, y + 58));
        DrawText(dc, isOpen ? "开" : "关", x + 50, y + 34, 14, isOpen ? Color.FromRgb(87, 244, 158) : Color.FromRgb(255, 118, 118), FontWeights.Bold);
    }

    private void DrawTank(DrawingContext dc, double x, double y, double temperature, string title)
    {
        var brush = new LinearGradientBrush(Color.FromRgb(92, 248, 242), Color.FromRgb(35, 214, 220), 0);
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(14, 126, 158)), 3);
        dc.DrawRoundedRectangle(brush, pen, new Rect(x, y, 160, 82), 40, 40);
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(120, 27, 185, 124)), null, new Rect(x + 132, y + 14, 10, 55), 4, 4);
        DrawText(dc, title, x + 38, y + 92, 14, Color.FromRgb(185, 238, 248), FontWeights.Bold);
        DrawTemperature(dc, x + 185, y + 24, temperature);
    }

    private void DrawPump(DrawingContext dc, double x, double y, bool running, string name)
    {
        var brush = new LinearGradientBrush(running ? Color.FromRgb(87, 246, 240) : Color.FromRgb(74, 111, 126), running ? Color.FromRgb(33, 214, 219) : Color.FromRgb(53, 77, 89), 0);
        var pen = new Pen(new SolidColorBrush(running ? Color.FromRgb(15, 116, 148) : Color.FromRgb(80, 108, 122)), 3);
        dc.DrawRoundedRectangle(brush, pen, new Rect(x, y, 145, 76), 12, 12);
        dc.DrawGeometry(brush, pen, Geometry.Parse($"M{x - 40},{y - 5} L{x},{y + 14} L{x},{y + 76} L{x - 40},{y + 58} Z"));
        dc.DrawRectangle(brush, pen, new Rect(x + 145, y + 22, 48, 28));
        DrawText(dc, $"{name} {(running ? "运行" : "停止")}", x + 24, y + 88, 14, running ? Color.FromRgb(87, 244, 158) : Color.FromRgb(170, 192, 204), FontWeights.Bold);
    }

    private void DrawVerticalTank(DrawingContext dc, double x, double y, double temperature)
    {
        var brush = new LinearGradientBrush(Color.FromRgb(85, 245, 239), Color.FromRgb(30, 210, 217), 90);
        dc.DrawRoundedRectangle(brush, new Pen(new SolidColorBrush(Color.FromRgb(15, 116, 148)), 3), new Rect(x, y, 55, 130), 24, 24);
        DrawText(dc, "进料罐", x + 2, y - 24, 14, Color.FromRgb(185, 238, 248), FontWeights.Bold);
        DrawTemperature(dc, x - 15, y + 150, temperature);
    }

    private void DrawMeter(DrawingContext dc, double x, double y, double temperature)
    {
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(15, 116, 148)), 3);
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(85, 245, 239)), pen, new Rect(x, y, 52, 112), 16, 16);
        dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(18, 56, 78)), pen, new Point(x + 35, y + 58), 10, 16);
        DrawText(dc, "计量罐", x + 2, y - 24, 14, Color.FromRgb(185, 238, 248), FontWeights.Bold);
        DrawTemperature(dc, x - 26, y + 150, temperature);
    }

    private void DrawTemperature(DrawingContext dc, double x, double y, double value)
    {
        dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(80, 51, 230, 238)), new Pen(new SolidColorBrush(Color.FromRgb(33, 159, 194)), 2), new Rect(x, y, 104, 36));
        DrawText(dc, $"{value:0.0}°C", x + 18, y + 6, 21, Color.FromRgb(91, 248, 241), FontWeights.Bold);
    }

    private void DrawStateBadge(DrawingContext dc, double x, double y, string text, Color color)
    {
        dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromArgb(60, color.R, color.G, color.B)), new Pen(new SolidColorBrush(color), 2), new Rect(x, y, 112, 32), 4, 4);
        dc.DrawEllipse(new SolidColorBrush(color), null, new Point(x + 18, y + 16), 7, 7);
        DrawText(dc, text, x + 34, y + 7, 14, color, FontWeights.Bold);
    }

    private void DrawText(DrawingContext dc, string text, double x, double y, double size, Color color, FontWeight weight)
    {
        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(typeface.FontFamily, FontStyles.Normal, weight, FontStretches.Normal),
            size,
            new SolidColorBrush(color),
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(formatted, new Point(x, y));
    }
}
