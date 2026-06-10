using ScadaTemplate.Infrastructure;

namespace ScadaTemplate.Models;

public sealed class DeviceStatus : ObservableObject
{
    private StatusKind status;
    private double value;

    public DeviceStatus(string name, StatusKind status, string description, double value = 0)
    {
        Name = name;
        this.status = status;
        Description = description;
        this.value = value;
    }

    public string Name { get; }

    public string Description { get; }

    public StatusKind Status
    {
        get => status;
        set
        {
            if (SetProperty(ref status, value))
            {
                OnPropertyChanged(nameof(StatusText));
            }
        }
    }

    public double Value
    {
        get => value;
        set => SetProperty(ref this.value, value);
    }

    public string StatusText => Status switch
    {
        StatusKind.Normal => "运行",
        StatusKind.Warning => "预警",
        StatusKind.Error => "故障",
        _ => "离线"
    };
}
