using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightNumberField : Grid
{
    private readonly Border _surface;
    private readonly TextBlock _display;
    private readonly TextBox _edit;
    private readonly Action<float> _changed;
    private readonly float _step;
    private float _value;
    private float _startValue;
    private double _startX;
    private bool _pressed;
    private bool _dragged;
    private bool _editing;

    public RightNumberField(string axis, float initial,
        float step, Action<float> changed)
    {
        _value = initial;
        _step = step;
        _changed = changed;
        Height = 25;
        Margin = new Thickness(8, 0);
        ColumnDefinitions = new ColumnDefinitions("22,*,90");
        var label = new TextBlock
        {
            Text = axis, FontSize = 11,
            Foreground = EditorTheme.TextSub,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _display = new TextBlock
        {
            Text = TransformNumber.Display(initial), FontSize = 12,
            Foreground = EditorTheme.TextMain,
            TextAlignment = TextAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0)
        };
        _surface = new Border
        {
            Height = 22, Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3), Child = _display
        };
        ToolTip.SetTip(_surface, "单击编辑；水平拖动调整数值");
        _edit = new TextBox
        {
            Text = TransformNumber.Display(initial),
            Height = 22, FontSize = 12, IsVisible = false,
            Padding = new Thickness(5, 1),
            Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Accent,
            BorderThickness = new Thickness(1),
            TextAlignment = TextAlignment.Right
        };
        Grid.SetColumn(_surface, 2);
        Grid.SetColumn(_edit, 2);
        Children.Add(label);
        Children.Add(_surface);
        Children.Add(_edit);
        WireInput();
    }
}