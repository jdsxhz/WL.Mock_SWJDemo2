using System.Windows;
using System.Windows.Controls;
using ScadaTemplate.Models;

namespace ScadaTemplate.Controls;

public sealed class StatusLamp : Control
{
    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(StatusKind), typeof(StatusLamp),
            new FrameworkPropertyMetadata(StatusKind.Offline, FrameworkPropertyMetadataOptions.AffectsRender));

    static StatusLamp()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusLamp), new FrameworkPropertyMetadata(typeof(StatusLamp)));
    }

    public StatusKind Status
    {
        get => (StatusKind)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
}
