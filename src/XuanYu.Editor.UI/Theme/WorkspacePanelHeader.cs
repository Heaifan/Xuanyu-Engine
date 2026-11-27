using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace XuanYu.Editor.UI;

internal static class WorkspacePanelHeader
{
    public static TextBlock CreateLabel(string text) => new()
    {
        Text = text,
        FontSize = 11,
        Foreground = EditorTheme.TextSub,
        VerticalAlignment = VerticalAlignment.Center
    };

    public static Border Create(string text) => Create(CreateLabel(text));

    public static Border Create(TextBlock label) => new()
    {
        Height = 26,
        Background = EditorTheme.PanelHeaderBg,
        BorderBrush = EditorTheme.Border,
        BorderThickness = new Thickness(0, 0, 0, 1),
        Padding = new Thickness(10, 0),
        Child = label
    };
}