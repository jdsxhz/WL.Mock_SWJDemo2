using ScadaTemplate.Infrastructure;

namespace ScadaTemplate.Models;

public sealed class ParameterItem : ObservableObject
{
    private double value;

    public ParameterItem(string name, double value, string unit, double minimum, double maximum)
    {
        Name = name;
        this.value = value;
        Unit = unit;
        Minimum = minimum;
        Maximum = maximum;
    }

    public string Name { get; }

    public double Value
    {
        get => value;
        set => SetProperty(ref this.value, value);
    }

    public string Unit { get; }

    public double Minimum { get; }

    public double Maximum { get; }
}
