using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using XuanYu.Core.Geodesy;

namespace XuanYu.Editor.UI;

internal sealed class GeoEntityTransformDialog : Window
{
    private readonly TextBox[] _fields = [new(), new(), new()];
    private readonly TextBlock _status = new();
    private readonly Key _operation;
    public EnuPosition Value { get; private set; }

    public GeoEntityTransformDialog(Key operation)
    {
        _operation = operation;
        Title = operation == Key.G ? "G · 平移（m）" : operation == Key.R ? "R · 旋转（°）" : "S · 缩放倍率";
        Width = 320; SizeToContent = SizeToContent.Height;
        CanResize = false; WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var panel = new StackPanel { Margin = new Thickness(12), Spacing = 6 };
        string[] axes = operation == Key.S ? ["模型 X", "模型 Y", "模型 Z"] : ["东 E / X", "北 N / Y", "上 U / Z"];
        for (int i = 0; i < 3; i++)
        {
            panel.Children.Add(new TextBlock { Text = axes[i] });
            _fields[i].Text = operation == Key.S ? "1" : "0";
            panel.Children.Add(_fields[i]);
        }
        panel.Children.Add(_status);
        var apply = new Button { Content = "应用（Enter）" };
        var cancel = new Button { Content = "取消（Esc）" };
        apply.Click += (_, _) => Apply();
        cancel.Click += (_, _) => Close(false);
        panel.Children.Add(new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal,
            Spacing = 8, Children = { apply, cancel } });
        Content = panel;
        Opened += (_, _) => _fields[0].Focus();
        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter) { Apply(); e.Handled = true; }
            else if (e.Key == Key.Escape) { Close(false); e.Handled = true; }
        };
    }

    private void Apply()
    {
        var values = new double[3];
        for (int i = 0; i < 3; i++)
        {
            if (!double.TryParse(_fields[i].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]) ||
                !double.IsFinite(values[i]) || (_operation != Key.G && !float.IsFinite((float)values[i])) ||
                (_operation == Key.S && values[i] <= 0d))
            { _status.Text = "请输入有效数字；缩放倍率必须大于0。"; return; }
        }
        Value = new(values[0], values[1], values[2]);
        Close(true);
    }
}