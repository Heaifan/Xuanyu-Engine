
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed class ViewportCompositionHost : Grid
{
    private readonly ViewportNavigationPanel _navigation = new()
    {
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Top,
        Margin = new Thickness(0, 12, 12, 0)
    };

    public ViewportCompositionSurface Scene { get; } = new();
    public Grid Overlay { get; } = new() { ClipToBounds = true };
    public event Action<bool>? PanModeChanged;

    public ViewportCompositionHost()
    {
        Background = Brushes.Transparent;
        ClipToBounds = true;
        Scene.IsHitTestVisible = false;
        Children.Add(Scene);
        Overlay.Children.Add(_navigation);
        Children.Add(Overlay);
        _navigation.PanModeChanged += enabled => PanModeChanged?.Invoke(enabled);
    }

    public void BindNavigation(Workbench workbench) => _navigation.Bind(workbench);
}
