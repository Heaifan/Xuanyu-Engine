using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftTerrainPanel
{
    private readonly StackPanel _dataset = new();
    private readonly StackPanel _tiles = new();
    private readonly RotateTransform _folderRotation = new();
    private readonly TextBlock _folderText = new() { FontSize = 12 };
    private readonly List<(TerrainDemEntry Entry, Border Row, TextBlock Text)> _tileRows = [];
    private Border _folderRow = null!;
    private IReadOnlyList<TerrainDemEntry>? _shownEntries;
    private bool _folderExpanded = true;

    private void BuildDatasetTree()
    {
        Control arrow = LeftSvgIcon.Create("Controls/chevron-down", 12);
        arrow.RenderTransformOrigin = RelativePoint.Center;
        arrow.RenderTransform = _folderRotation;
        _folderRow = DatasetRow(1, true, arrow, "Nodes/folder", _folderText, () =>
        {
            _folderExpanded = !_folderExpanded;
            _wb.SelectTerrain(true);
        });
        _dataset.Children.Add(_folderRow);
        _dataset.Children.Add(_tiles);
        Children.Add(_dataset);
    }

    private void RefreshDatasetTree()
    {
        IReadOnlyList<TerrainDemEntry> entries = _wb.TerrainDemEntries;
        if (!ReferenceEquals(entries, _shownEntries))
        {
            _shownEntries = entries;
            _tiles.Children.Clear();
            _tileRows.Clear();
            _folderExpanded = true;
            _expanded = true;
            for (int i = 0; i < entries.Count; i++) AddTileRow(entries[i], i == entries.Count - 1);
        }
        _dataset.IsVisible = _expanded && entries.Count > 0;
        _tiles.IsVisible = _folderExpanded;
        _folderRotation.Angle = _folderExpanded ? 0 : -90;
        _folderText.Text = $"NASADEM 数据集（{entries.Count} 片）";
        SetDatasetRow(_folderRow, _folderText, _wb.TerrainSelected && _wb.SelectedTerrainDem is null);
        foreach (var item in _tileRows)
            SetDatasetRow(item.Row, item.Text, ReferenceEquals(item.Entry, _wb.SelectedTerrainDem));
    }

    private void AddTileRow(TerrainDemEntry entry, bool last)
    {
        var text = new TextBlock { Text = entry.TileId, FontSize = 12 };
        Border row = DatasetRow(2, last, null, "Terrain/heightfield", text,
            () => _wb.SelectTerrainDem(entry.TileId));
        ToolTip.SetTip(row, entry.Path);
        _tiles.Children.Add(row);
        _tileRows.Add((entry, row, text));
    }

    private static void SetDatasetRow(Border row, TextBlock text, bool selected)
    {
        row.Background = selected ? SelectedBrush : row.IsPointerOver ? HoverBrush : Brushes.Transparent;
        text.Foreground = selected ? EditorTheme.Accent : EditorTheme.TextMain;
    }
}