using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportBox
{
    internal void BindNavigation(Workbench workbench)
    {
        var layout = (Grid)Child!;
        var popup = (Popup)layout.Children[1];
        var panel = (ViewportNavigationPanel)popup.Child!;
        panel.Bind(workbench);
        panel.PanModeChanged += enabled =>
        {
            _windows?.SetPanMode(enabled);
            _linux?.SetPanMode(enabled);
        };
    }
}