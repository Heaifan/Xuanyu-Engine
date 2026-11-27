using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private void AddContourControls()
    {
        if (!_wb.IsGeoTerrainPreview && !_wb.HasLocalTerrainContours) return;
        AddGroup("地形显示", _showContours, () => _showContours = !_showContours);
        if (!_showContours) return;
        var toggle = new CheckBox
        {
            Content = "启用等高线",
            IsChecked = _wb.TerrainContoursEnabled,
            FontSize = 11,
            Margin = new Thickness(8, 4),
            Foreground = EditorTheme.TextMain
        };
        toggle.PropertyChanged += (_, e) =>
        {
            if (e.Property == ToggleButton.IsCheckedProperty)
                _wb.SetTerrainContoursEnabled(toggle.IsChecked == true);
        };
        _rows.Children.Add(toggle);
        var labels = new CheckBox
        {
            Content = "显示高程数值",
            IsChecked = _wb.TerrainContourLabelsEnabled,
            FontSize = 11,
            Margin = new Thickness(8, 0, 8, 4),
            Foreground = EditorTheme.TextMain
        };
        labels.PropertyChanged += (_, e) =>
        {
            if (e.Property == ToggleButton.IsCheckedProperty)
                _wb.SetTerrainContourLabelsEnabled(labels.IsChecked == true);
        };
        _rows.Children.Add(labels);
        var shade = new CheckBox
        {
            Content = "地形晕渲", IsChecked = _wb.TerrainHillshadeEnabled,
            FontSize = 11, Margin = new Thickness(8, 0, 8, 4),
            Foreground = EditorTheme.TextMain
        };
        shade.PropertyChanged += (_, e) =>
        {
            if (e.Property == ToggleButton.IsCheckedProperty)
                _wb.SetTerrainHillshadeEnabled(shade.IsChecked == true);
        };
        _rows.Children.Add(shade);
        AddPair("当前间距", $"{_wb.TerrainContourInterval} m");
        AddPair("显示规则", _wb.IsGeoTerrainPreview
            ? "六档自适应 · 远景淡化" : "1m 起 · 自适应等高距");
    }
}