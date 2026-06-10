using ScadaTemplate.Infrastructure;

namespace ScadaTemplate.Models;

public sealed class MessageItem : ObservableObject
{
    private bool isAcknowledged;

    public MessageItem(StatusKind level, DateTime time, string name, string content)
    {
        Level = level;
        Time = time;
        Name = name;
        Content = content;
    }

    public StatusKind Level { get; }

    public DateTime Time { get; }

    public string TimeText => Time.ToString("yyyy-MM-dd HH:mm:ss");

    public string Name { get; }

    public string Content { get; }

    public bool IsAcknowledged
    {
        get => isAcknowledged;
        set => SetProperty(ref isAcknowledged, value);
    }
}
