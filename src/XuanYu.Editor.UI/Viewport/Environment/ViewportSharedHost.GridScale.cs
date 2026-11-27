using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private readonly TextBlock _gridScale = new()
    {
        FontSize = 11,
        Foreground = EditorTheme.TextSub,
        Background = EditorTheme.PanelBg,
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Thickness(0, 0, 12, 12),
        Padding = new Thickness(8, 4),
        IsHitTestVisible = false,
        IsVisible = false
    };

    private void UpdateGridScale()
    {
        bool enabled = _wb.CurrentId switch
        {
            PageId.World => _wb.WorldGridEnabled,
            PageId.Terrain => _wb.TerrainGridEnabled,
            _ => _wb.WorldGridEnabled
        };
        string label = enabled && !_wb.IsSimulating
            ? _wb.GridScaleText : string.Empty;
        _gridScale.Text = label;
        _gridScale.IsVisible = !string.IsNullOrWhiteSpace(label);
    }
}