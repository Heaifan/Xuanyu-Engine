using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using XuanYu.Editor.Preview;
using XuanYu.Editor.UI.Right;

namespace XuanYu.Editor.UI;

public sealed partial class EditorWindow
{
    private enum TerrainRightTab { Data, Object }

    private PreviewScene? _boundTerrainScene;
    private Border? _terrainRoot;
    private ContentControl? _terrainContent;
    private Border? _terrainDataTab;
    private Border? _terrainObjectTab;
    private RightTerrainInspector? _terrainDataInspector;
    private RightPreviewInspector? _terrainObjectInspector;
    private TerrainRightTab _terrainTab = TerrainRightTab.Data;
    private void SyncTerrainInspector()
    {
        PreviewScene scene = _wb.TerrainScene;
        if (!ReferenceEquals(_boundTerrainScene, scene) || _terrainRoot is null)
            BuildTerrainInspector(scene);
        _terrainDataInspector!.Refresh();
        _terrainContent!.Content = _terrainTab == TerrainRightTab.Data
            ? _terrainDataInspector : _terrainObjectInspector;
        SetTabState(_terrainDataTab!, _terrainTab == TerrainRightTab.Data);
        SetTabState(_terrainObjectTab!, _terrainTab == TerrainRightTab.Object);
        _rightHost.Content = _terrainRoot;
    }
    private void BuildTerrainInspector(PreviewScene scene)
    {
        _boundTerrainScene = scene;
        _terrainTab = TerrainRightTab.Data;
        _terrainDataInspector = new RightTerrainInspector(_wb);
        _terrainObjectInspector = new RightPreviewInspector(scene);
        _terrainContent = new ContentControl();
        _terrainDataTab = CreateTerrainTab("地形数据", TerrainRightTab.Data);
        _terrainObjectTab = CreateTerrainTab("对象属性", TerrainRightTab.Object);
        var tabs = new Grid { ColumnDefinitions = new("84,84,*") };
        tabs.Children.Add(_terrainDataTab);
        Grid.SetColumn(_terrainObjectTab, 1);
        tabs.Children.Add(_terrainObjectTab);
        var header = new Border
        {
            Background = EditorTheme.PanelHeaderBg, BorderBrush = EditorTheme.Border,
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(10, 0),
            Child = new TextBlock
            {
                Text = "TERRAIN / 地形属性", FontSize = 10, Foreground = EditorTheme.TextMuted,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            }
        };
        var root = new Grid { RowDefinitions = new("26,34,*") };
        Grid.SetRow(tabs, 1);
        Grid.SetRow(_terrainContent, 2);
        root.Children.Add(header);
        root.Children.Add(tabs);
        root.Children.Add(_terrainContent);
        _terrainRoot = new Border { Child = root };
    }
    private Border CreateTerrainTab(string label, TerrainRightTab tab)
    {
        var item = new Border
        {
            Height = 34,
            BorderBrush = EditorTheme.Accent,
            Background = Brushes.Transparent,
            Child = new TextBlock
            {
                Text = label, FontSize = 12,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            }
        };
        item.PointerPressed += (_, e) =>
        {
            if (_terrainTab == tab) return;
            _terrainTab = tab;
            if (_boundTerrainScene is { Objects.Count: > 0 } scene)
                scene.Select(tab == TerrainRightTab.Object
                    ? scene.Objects[0].Id : null);
            SyncTerrainInspector();
            e.Handled = true;
        };
        return item;
    }

    private static void SetTabState(Border tab, bool active)
    {
        tab.BorderThickness = active
            ? new Thickness(0, 0, 0, 2) : new Thickness(0);
        if (tab.Child is TextBlock text)
            text.Foreground = active
                ? EditorTheme.Accent : EditorTheme.TextMuted;
    }
}