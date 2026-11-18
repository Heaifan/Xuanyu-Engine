using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftTerrainPanel : StackPanel
{
    private readonly Workbench _wb;
    private readonly Border _mapRow;
    private readonly Border _demRow;
    private readonly Control _chevron;
    private readonly RotateTransform _chevronRotation = new();
    private readonly TextBlock _mapText = new() { FontSize = 12 };
    private readonly TextBlock _demText = new()
    { FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis };
    private bool _expanded = true;

    public LeftTerrainPanel(Workbench wb)
    {
        _wb = wb;
        Spacing = 0;
        Children.Add(new TextBlock
        {
            Text = "场景结构", FontFamily = new FontFamily("Microsoft YaHei UI"),
            FontWeight = FontWeight.SemiBold, FontSize = 11,
            Foreground = EditorTheme.TextSub,
            Margin = new Thickness(4, 8, 0, 4)
        });
        _mapText.VerticalAlignment = VerticalAlignment.Center;
        _demText.VerticalAlignment = VerticalAlignment.Center;
        _chevron = LeftSvgIcon.Create("Controls/chevron-down", 12);
        _mapRow = BuildMapRow();
        _demRow = BuildDemRow();
        Children.Add(_mapRow);
        Children.Add(_demRow);
        BuildDatasetTree();
        ContextMenu = CreateTerrainMenu();
        Refresh();
    }

    public void Refresh()
    {
        RefreshDatasetTree();
        _mapText.Text = _wb.TerrainMultiSourceLabel is null
            ? _wb.TerrainMap.Name : "NASADEM 多瓦片地形";
        string name = _wb.TerrainNodeText;
        _demText.Text = name.Length > 27
            ? $"{name[..12]}…{name[^14..]}" : name;
        ToolTip.SetTip(_demRow, _wb.TerrainMultiSourceLabel ?? _wb.TerrainSourcePath ?? name);
        _mapRow.Background = !_wb.TerrainSelected ? SelectedBrush :
            _mapRow.IsPointerOver ? HoverBrush : Brushes.Transparent;
        _demRow.Background = _wb.TerrainSelected ? SelectedBrush :
            _demRow.IsPointerOver ? HoverBrush : Brushes.Transparent;
        _mapText.Foreground = !_wb.TerrainSelected ? EditorTheme.Accent : EditorTheme.TextMain;
        _demText.Foreground = _wb.TerrainSelected ? EditorTheme.Accent : EditorTheme.TextMain;
        _demRow.IsVisible = _expanded && _wb.TerrainDemEntries.Count == 0;
        _chevronRotation.Angle = _expanded ? 0 : -90;
    }
}