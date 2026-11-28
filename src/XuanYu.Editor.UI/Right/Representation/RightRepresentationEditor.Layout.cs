using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightRepresentationEditor
{
    private void BuildRows()
    {
        HorizontalAlignment = HorizontalAlignment.Stretch;
        Margin = new Thickness(0);
        Spacing = 0;
        var body = FormBody();
        var apply = new Button { Content = "应用尺寸" };
        apply.Click += (_, _) => ApplySize();
        body.Children.Add(Field("表现形式", _choice));
        body.Children.Add(Field("单位图标", _icon));
        _style.Children.Add(Field("尺寸模式", _mode));
        _style.Children.Add(Field(_sizeLabel, _size));
        _style.Children.Add(Field("", apply));
        body.Children.Add(_style);
        StyleHint(_error);
        body.Children.Add(_error);
        Children.Add(body);
        Children.Add(CreateAdvanced());
    }

    internal static StackPanel FormBody() => new() { Margin = new Thickness(8, 3), Spacing = 2 };

    internal static void StyleHint(TextBlock text)
    {
        text.FontSize = 11;
        text.Foreground = EditorTheme.TextSub;
        text.TextWrapping = TextWrapping.Wrap;
    }

    internal static Grid Field(string title, Control input) => Field(new TextBlock { Text = title }, input);

    private static Grid Field(TextBlock label, Control input)
    {
        var row = new Grid { ColumnDefinitions = new("74,*"), Height = 27 };
        label.FontSize = 11;
        label.Foreground = EditorTheme.TextSub;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextTrimming = TextTrimming.CharacterEllipsis;
        StyleInput(input);
        Grid.SetColumn(input, 1);
        row.Children.Add(label);
        row.Children.Add(input);
        return row;
    }

    private static void StyleInput(Control input)
    {
        input.Height = 25;
        input.MinHeight = 0;
        input.MinWidth = 0;
        input.HorizontalAlignment = HorizontalAlignment.Stretch;
        input.VerticalAlignment = VerticalAlignment.Center;
        if (input is ComboBox combo)
        {
            combo.FontSize = 11; combo.Padding = new Thickness(5, 0);
            combo.Background = EditorTheme.PanelHeaderBg; combo.Foreground = EditorTheme.TextMain;
            combo.BorderBrush = EditorTheme.Border; combo.BorderThickness = new Thickness(1);
            combo.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            combo.VerticalContentAlignment = VerticalAlignment.Center;
        }
        else if (input is TextBox text)
        {
            text.FontSize = 11; text.Padding = new Thickness(5, 0);
            text.Background = EditorTheme.PanelHeaderBg; text.Foreground = EditorTheme.TextMain;
            text.BorderBrush = EditorTheme.Border; text.BorderThickness = new Thickness(1);
            text.TextAlignment = TextAlignment.Right;
            text.VerticalContentAlignment = VerticalAlignment.Center;
        }
        else if (input is Button button)
        {
            button.FontSize = 11; button.Padding = new Thickness(5, 0);
            button.Background = EditorTheme.PanelHeaderBg; button.Foreground = EditorTheme.TextMain;
            button.BorderBrush = EditorTheme.Border; button.BorderThickness = new Thickness(1);
            button.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            button.VerticalContentAlignment = VerticalAlignment.Center;
        }
    }
}