using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftWorkspacePanel
{
    private LeftTerrainPanel? _terrainPanel;
    private Avalonia.Controls.ContextMenu? _terrainMenu;

    public void BindTerrain(Workbench workbench)
    {
        _terrainPanel = new LeftTerrainPanel(workbench);
        _terrainMenu = _terrainPanel.CreateTerrainMenu();
    }

    public void SetPage(PageInfo info, string detail)
    {
        bool isWorld = info.Id == PageId.World;
        bool isTerrain = info.Id == PageId.Terrain;
        _headerLabel.Text = isWorld ? "场景树" : info.LeftTitle;
        _scroll.ContextMenu = isWorld ? _sceneMenu : isTerrain ? _terrainMenu : null;
        _infoDetail.Text = detail;

        if (isWorld)
        {
            if (!ReferenceEquals(_scroll.Content, _tree))
                _scroll.Content = _tree;
            _tree.Refresh();
        }
        else if (isTerrain)
        {
            _terrainPanel!.Refresh();
            if (!ReferenceEquals(_scroll.Content, _terrainPanel))
                _scroll.Content = _terrainPanel;
        }
        else if (!ReferenceEquals(_scroll.Content, _infoPanel))
            _scroll.Content = _infoPanel;
    }
}