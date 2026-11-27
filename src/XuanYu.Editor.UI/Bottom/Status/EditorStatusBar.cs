using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed class EditorStatusBar : Border
{
    private readonly TextBlock _state = CreateText();
    private readonly TextBlock _gpu = CreateText();

    public EditorStatusBar()
    {
        Background = EditorTheme.PanelHeaderBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(0, 1, 0, 0);
        Padding = new Thickness(10, 0);
        _gpu.TextAlignment = TextAlignment.Right;
        _gpu.TextTrimming = TextTrimming.CharacterEllipsis;
        var row = new Grid
        {
            ColumnDefinitions = new("Auto,*"),
            ColumnSpacing = 16
        };
        Grid.SetColumn(_gpu, 1);
        row.Children.Add(_state);
        row.Children.Add(_gpu);
        Child = row;
    }

    public void Refresh(Workbench wb)
    {
        string mode = wb.IsSimulating
            ? $"模拟态 · {wb.Simulation.ElapsedSeconds:0.0} s"
            : "编辑态";
        _state.Text = $"{wb.Current.Title}  |  {mode}";
        _gpu.Text = wb.ViewportNote;
        ToolTip.SetTip(_gpu, wb.ViewportNote);
    }

    private static TextBlock CreateText() => new()
    {
        FontSize = 11,
        Foreground = EditorTheme.TextSub,
        VerticalAlignment = VerticalAlignment.Center
    };
}