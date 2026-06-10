using ScadaTemplate.Infrastructure;

namespace ScadaTemplate.Models;

public sealed class OptionItem : ObservableObject
{
    private StatusKind status;
    private bool isEnabled;

    public OptionItem(string name, StatusKind status, bool isEnabled = true)
    {
        Name = name;
        this.status = status;
        this.isEnabled = isEnabled;
    }

    public string Name { get; }

    public StatusKind Status
    {
        get => status;
        set => SetProperty(ref status, value);
    }

    public bool IsEnabled
    {
        get => isEnabled;
        set => SetProperty(ref isEnabled, value);
    }
}
