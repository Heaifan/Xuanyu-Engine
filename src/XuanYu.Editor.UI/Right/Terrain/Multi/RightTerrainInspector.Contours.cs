using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private void AddContourControls()
    {
        if (!_wb.IsGeoTerrainPreview) return;
        AddGroup("等高线", _showContours, () => _showContours = !_showContours);
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
        AddPair("当前间距", $"{_wb.TerrainContourInterval} m");
        AddPair("显示规则", "六档自适应 · 远景淡化");
    }
}