using Avalonia;
using Avalonia.Controls;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed partial class TopToolRow
{
    private readonly ComboBox _mode = new()
    {
        Width = 120, Height = 28, FontSize = 12,
        ItemsSource = new[] { "对象模式", "地形编辑模式" }
    };
    private readonly Dictionary<TerrainToolGroup, Button> _groups = [];
    private Button _undo = null!, _redo = null!, _grid = null!;
    private Button _contours = null!, _shade = null!;

    private void BuildTerrain()
    {
        _mode.SelectionChanged += (_, _) =>
        {
            if (_mode.SelectedIndex < 0 || !_wb.CanEditLocalTerrain ||
                _wb.CurrentId != PageId.Terrain) return;
            _wb.SetTerrainMode(_mode.SelectedIndex == 1
                ? TerrainWorkspaceMode.Edit : TerrainWorkspaceMode.Object);
        };
        _terrain.Children.Add(_mode);
        Divide(_terrain);
        AddGroup(TerrainToolGroup.Generation, "生成");
        AddGroup(TerrainToolGroup.Sculpt, "雕刻");
        AddGroup(TerrainToolGroup.Evolution, "演化");
        AddGroup(TerrainToolGroup.Hydrology, "水文");
        AddGroup(TerrainToolGroup.Surface, "地表");
        AddGroup(TerrainToolGroup.Engineering, "工程");
        AddGroup(TerrainToolGroup.Analysis, "分析");
        Divide(_terrain);
        _undo = Add(_terrain, "撤销", _wb.UndoTerrain);
        _redo = Add(_terrain, "重做", _wb.RedoTerrain);
        _grid = Add(_terrain, "网格", () => _wb.SetTerrainGridEnabled(!_wb.TerrainGridEnabled));
        _contours = Add(_terrain, "等高线", () => _wb.SetTerrainContoursEnabled(!_wb.TerrainContoursEnabled));
        _shade = Add(_terrain, "晕渲", () => _wb.SetTerrainHillshadeEnabled(!_wb.TerrainHillshadeEnabled));
    }

    private void AddGroup(TerrainToolGroup group, string label)
    {
        var button = Add(_terrain, label, () => _wb.SelectTerrainToolGroup(group));
        _groups.Add(group, button);
        if (group is not (TerrainToolGroup.Generation or TerrainToolGroup.Sculpt or TerrainToolGroup.Evolution))
            ToolTip.SetTip(button, "暂未开放");
    }

    private void RefreshTerrain()
    {
        _mode.IsEnabled = _wb.CanEditLocalTerrain;
        int index = _wb.IsTerrainEditMode && _wb.CanEditLocalTerrain ? 1 : 0;
        if (_mode.SelectedIndex != index) _mode.SelectedIndex = index;
        foreach (var (group, button) in _groups)
        {
            button.IsEnabled = _wb.CanSelectTerrainToolGroup(group);
            Paint(button, button.IsEnabled && group == _wb.ActiveTerrainToolGroup);
        }
        _undo.IsEnabled = _wb.CanUndoTerrain;
        _redo.IsEnabled = _wb.CanRedoTerrain;
        _contours.IsEnabled = _wb.IsGeoTerrainPreview || _wb.HasLocalTerrainContours;
        _shade.IsEnabled = _contours.IsEnabled;
        Paint(_grid, _wb.TerrainGridEnabled);
        Paint(_contours, _wb.TerrainContoursEnabled);
        Paint(_shade, _wb.TerrainHillshadeEnabled);
    }
}