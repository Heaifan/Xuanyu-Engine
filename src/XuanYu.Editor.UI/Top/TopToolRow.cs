using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed class TopToolRow : Border
{
    private readonly Workbench _wb;
    private readonly StackPanel _terrain = new() { Orientation = Orientation.Horizontal, Spacing = 5 };
    private readonly StackPanel _world = new() { Orientation = Orientation.Horizontal, Spacing = 5 };
    private readonly ComboBox _mode = new()
    {
        Width = 120, Height = 28, FontSize = 12,
        ItemsSource = new[] { "对象模式", "地形编辑模式" }
    };
    private readonly Button _undo, _redo, _grid, _contours, _shade, _worldGrid, _sky;
    private readonly TextBlock _context = new()
    {
        FontSize = 11, Foreground = EditorTheme.TextSub,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Right
    };

    public TopToolRow(Workbench wb)
    {
        _wb = wb;
        Background = EditorTheme.PanelHeaderBg;
        BorderBrush = EditorTheme.Border;
        BorderThickness = new Thickness(0, 1, 0, 0);
        Padding = new Thickness(8, 3);
        _mode.SelectionChanged += (_, _) =>
        {
            if (_mode.SelectedIndex < 0 || !_wb.CanEditLocalTerrain || _wb.CurrentId != PageId.Terrain) return;
            _wb.SetTerrainMode(_mode.SelectedIndex == 1 ? TerrainWorkspaceMode.Edit : TerrainWorkspaceMode.Object);
        };
        _terrain.Children.Add(_mode);
        _undo = Add(_terrain, "撤销", wb.UndoTerrain);
        _redo = Add(_terrain, "重做", wb.RedoTerrain);
        _grid = Add(_terrain, "网格", () => wb.SetTerrainGridEnabled(!wb.TerrainGridEnabled));
        _contours = Add(_terrain, "等高线", () => wb.SetTerrainContoursEnabled(!wb.TerrainContoursEnabled));
        _shade = Add(_terrain, "晕渲", () => wb.SetTerrainHillshadeEnabled(!wb.TerrainHillshadeEnabled));
        _worldGrid = Add(_world, "网格", () => wb.SetWorldGridEnabled(!wb.WorldGridEnabled));
        _sky = Add(_world, "天空", () => wb.SetSkyEnabled(!wb.SkyEnabled));
        var row = new Grid { ColumnDefinitions = new("*,Auto") };
        row.Children.Add(_terrain);
        row.Children.Add(_world);
        Grid.SetColumn(_context, 1);
        row.Children.Add(_context);
        Child = row;
    }

    public void Refresh()
    {
        _terrain.IsVisible = _wb.CurrentId == PageId.Terrain;
        _world.IsVisible = _wb.CurrentId == PageId.World;
        _mode.IsEnabled = _wb.CanEditLocalTerrain;
        int index = _wb.IsTerrainEditMode && _wb.CanEditLocalTerrain ? 1 : 0;
        if (_mode.SelectedIndex != index) _mode.SelectedIndex = index;
        _undo.IsEnabled = _wb.CanUndoTerrain;
        _redo.IsEnabled = _wb.CanRedoTerrain;
        _contours.IsEnabled = _wb.IsGeoTerrainPreview || _wb.HasLocalTerrainContours;
        _shade.IsEnabled = _contours.IsEnabled;
        Paint(_grid, _wb.TerrainGridEnabled);
        Paint(_contours, _wb.TerrainContoursEnabled);
        Paint(_shade, _wb.TerrainHillshadeEnabled);
        Paint(_worldGrid, _wb.WorldGridEnabled);
        Paint(_sky, _wb.SkyEnabled);
        _context.Text = _wb.CurrentId == PageId.Terrain && !_wb.CanEditLocalTerrain
            ? "地形预览 · 不可雕刻" : _wb.Current.Title;
    }

    private static Button Add(StackPanel panel, string label, Action action)
    {
        var button = new Button { Content = label, FontSize = 12, Height = 28,
            Padding = new Thickness(9, 3), Focusable = false };
        button.Click += (_, _) => action();
        panel.Children.Add(button);
        return button;
    }

    private static void Paint(Button button, bool active)
    {
        button.Background = active ? EditorTheme.AccentSoft : EditorTheme.TabIdle;
        button.Foreground = active ? EditorTheme.Accent : EditorTheme.TextSub;
    }
}