using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

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
        _edit = new TextBox
        {
            Text = TransformNumber.Display(initial),
            FontSize = 12, Height = 20, MinHeight = 0, MinWidth = 0,
            Padding = new Thickness(5, 0),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            TextAlignment = TextAlignment.Right,
            VerticalContentAlignment = VerticalAlignment.Center
        };
        _surface = new Border
        {
            Height = 22, Background = EditorTheme.PanelHeaderBg,
            BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(3),
            ClipToBounds = true, Child = _display
        };
        Styles.Add(new Style(x => x.OfType<TextBox>().Class(":focus")
            .Template().OfType<Border>().Name("PART_BorderElement"))
        {
            Setters =
            {
                new Setter(Border.BackgroundProperty, Brushes.Transparent),
                new Setter(Border.BorderBrushProperty, Brushes.Transparent),
                new Setter(Border.BorderThicknessProperty, new Thickness(0))
            }
        });
        ToolTip.SetTip(_surface, "单击编辑；水平拖动调整数值");
        Grid.SetColumn(_surface, 2);
        Children.Add(label);
        Children.Add(_surface);
        WireInput();
    }
}