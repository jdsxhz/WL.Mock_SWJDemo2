using System.Windows;
using System.Windows.Controls;

namespace ScadaTemplate.Controls;

public sealed class TechPanel : HeaderedContentControl
{
    static TechPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TechPanel), new FrameworkPropertyMetadata(typeof(TechPanel)));
    }
}
