using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private void AddLocalSizeControls()
    {
        TerrainHeightField field = _wb.TerrainMap.HeightField;
        TextBox width = AddNumberInput("宽度 X(m)", field.WidthMeters);
        TextBox length = AddNumberInput("长度 Y(m)", field.LengthMeters);
        TextBox spacing = AddNumberInput("目标间距(m)", field.SpacingMeters);
        var estimate = new TextBlock
        {
            FontSize = 11, Foreground = EditorTheme.TextSub,
            Margin = new Thickness(4, 4, 0, 2),
            TextWrapping = TextWrapping.Wrap
        };
        var status = new TextBlock
        {
            FontSize = 10, Foreground = EditorTheme.TextMuted,
            TextWrapping = TextWrapping.Wrap
        };
        var apply = new Button
        {
            Content = "应用尺寸（重置高度）", FontSize = 11,
            Margin = new Thickness(0, 3, 0, 0)
        };
        _rows.Children.Add(estimate);
        _rows.Children.Add(apply);
        _rows.Children.Add(status);
        void UpdateEstimate() => estimate.Text =
            SamplePreview(width.Text, length.Text, spacing.Text);
        width.TextChanged += (_, _) => UpdateEstimate();
        length.TextChanged += (_, _) => UpdateEstimate();
        spacing.TextChanged += (_, _) => UpdateEstimate();
        apply.Click += (_, _) => ApplyLocalSize(
            width.Text, length.Text, spacing.Text, status);
        UpdateEstimate();
    }

    private TextBox AddNumberInput(string label, float value)
    {
        var input = new TextBox
        {
            Text = value.ToString("0.###", CultureInfo.InvariantCulture),
            FontSize = 11, Height = 27
        };
        var row = new Grid
        {
            ColumnDefinitions = new("85,*"), MinHeight = 31
        };
        row.Children.Add(new TextBlock
        {
            Text = label, FontSize = 11, Foreground = EditorTheme.TextSub,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        });
        Grid.SetColumn(input, 1);
        row.Children.Add(input);
        _rows.Children.Add(row);
        return input;
    }
}