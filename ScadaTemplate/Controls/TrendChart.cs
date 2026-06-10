using System.Windows;
using System.Windows.Controls;
using ScottPlot;
using ScottPlot.WPF;

namespace ScadaTemplate.Controls;

public sealed class TrendChart : ContentControl
{
    public static readonly DependencyProperty Temperature1Property = RegisterMetric(nameof(Temperature1), 14d);
    public static readonly DependencyProperty Temperature2Property = RegisterMetric(nameof(Temperature2), 14d);
    public static readonly DependencyProperty Level1Property = RegisterMetric(nameof(Level1), 70d);
    public static readonly DependencyProperty Level2Property = RegisterMetric(nameof(Level2), 65d);
    public static readonly DependencyProperty HoursProperty =
        DependencyProperty.Register(
            nameof(Hours),
            typeof(int),
            typeof(TrendChart),
            new FrameworkPropertyMetadata(1, OnHoursChanged));

    private const string ChineseFont = "Microsoft YaHei UI";
    private readonly WpfPlot plotControl = new();
    private readonly Queue<double> temperature1 = new();
    private readonly Queue<double> temperature2 = new();
    private readonly Queue<double> level1 = new();
    private readonly Queue<double> level2 = new();
    private bool initialized;

    public TrendChart()
    {
        Content = plotControl;
        Loaded += (_, _) =>
        {
            InitializePlot();
            SeedHistory();
            RenderPlot();
        };
    }

    public double Temperature1
    {
        get => (double)GetValue(Temperature1Property);
        set => SetValue(Temperature1Property, value);
    }

    public double Temperature2
    {
        get => (double)GetValue(Temperature2Property);
        set => SetValue(Temperature2Property, value);
    }

    public double Level1
    {
        get => (double)GetValue(Level1Property);
        set => SetValue(Level1Property, value);
    }

    public double Level2
    {
        get => (double)GetValue(Level2Property);
        set => SetValue(Level2Property, value);
    }

    public int Hours
    {
        get => (int)GetValue(HoursProperty);
        set => SetValue(HoursProperty, value);
    }

    private int MaximumPointCount => Hours switch
    {
        <= 1 => 60,
        <= 8 => 96,
        _ => 144
    };

    private static DependencyProperty RegisterMetric(string name, double defaultValue)
    {
        return DependencyProperty.Register(
            name,
            typeof(double),
            typeof(TrendChart),
            new FrameworkPropertyMetadata(defaultValue, OnMetricChanged));
    }

    private static void OnMetricChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TrendChart chart && chart.IsLoaded)
        {
            chart.AppendCurrentValues();
        }
    }

    private static void OnHoursChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TrendChart chart && chart.IsLoaded)
        {
            chart.ResizeHistory();
            chart.RenderPlot();
        }
    }

    private void InitializePlot()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        plotControl.Plot.FigureBackground.Color = Color.FromHex("#07192A");
        plotControl.Plot.DataBackground.Color = Color.FromHex("#0B2438");
        plotControl.Plot.Axes.Color(Color.FromHex("#8DBBD0"));
        plotControl.Plot.Grid.MajorLineColor = Color.FromHex("#24465B");

        plotControl.Plot.Title("过程量实时趋势");
        plotControl.Plot.XLabel("采样点");
        plotControl.Plot.YLabel("工程值");

        plotControl.Plot.Axes.Title.Label.FontName = ChineseFont;
        plotControl.Plot.Axes.Title.Label.SetBestFont();
        dynamic bottomAxis = plotControl.Plot.Axes.Bottom;
        bottomAxis.LabelFontName = ChineseFont;
        bottomAxis.LabelStyle.SetBestFont();
        plotControl.Plot.Axes.Bottom.TickLabelStyle.FontName = ChineseFont;
        dynamic leftAxis = plotControl.Plot.Axes.Left;
        leftAxis.LabelFontName = ChineseFont;
        leftAxis.LabelStyle.SetBestFont();
        plotControl.Plot.Axes.Left.TickLabelStyle.FontName = ChineseFont;
        plotControl.Plot.Legend.FontName = ChineseFont;
        plotControl.Plot.Legend.SetBestFontOnEachRender = true;
    }

    private void AppendCurrentValues()
    {
        InitializePlot();
        Enqueue(temperature1, Temperature1);
        Enqueue(temperature2, Temperature2);
        Enqueue(level1, Level1);
        Enqueue(level2, Level2);
        RenderPlot();
    }

    private void RenderPlot()
    {
        plotControl.Plot.Clear();
        AddSignal(temperature1, "#36F1F2", "罐1温度");
        AddSignal(temperature2, "#5B8CFF", "罐2温度");
        AddSignal(level1, "#8DFFBE", "过滤器1液位");
        AddSignal(level2, "#FCD640", "过滤器2液位");
        plotControl.Plot.ShowLegend();
        plotControl.Plot.Axes.AutoScale();
        plotControl.Refresh();
    }

    private void AddSignal(IEnumerable<double> values, string color, string legend)
    {
        var signal = plotControl.Plot.Add.Signal(values.ToArray());
        signal.Color = Color.FromHex(color);
        signal.LineWidth = 2;
        signal.LegendText = legend;
    }

    private void Enqueue(Queue<double> queue, double value)
    {
        queue.Enqueue(value);
        while (queue.Count > MaximumPointCount)
        {
            queue.Dequeue();
        }
    }

    private void SeedHistory()
    {
        if (temperature1.Count > 0)
        {
            return;
        }

        FillHistory(MaximumPointCount);
    }

    private void ResizeHistory()
    {
        foreach (var queue in new[] { temperature1, temperature2, level1, level2 })
        {
            while (queue.Count > MaximumPointCount)
            {
                queue.Dequeue();
            }
        }

        if (temperature1.Count < MaximumPointCount)
        {
            FillHistory(MaximumPointCount - temperature1.Count);
        }
    }

    private void FillHistory(int count)
    {
        var start = temperature1.Count;
        for (var i = 0; i < count; i++)
        {
            var phase = (start + i) / 8d;
            temperature1.Enqueue(Temperature1 + Math.Sin(phase) * .7);
            temperature2.Enqueue(Temperature2 + Math.Cos(phase * .9) * .6);
            level1.Enqueue(Level1 + Math.Sin(phase * .55) * 3.2);
            level2.Enqueue(Level2 + Math.Cos(phase * .6) * 2.8);
        }
    }
}
