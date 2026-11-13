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

    public void SetPage(PageInfo info, bool isWorld, bool isTerrain)
    {
        _tabs.IsVisible = isWorld || isTerrain;
        _footerLabel.Text = isWorld ? "世界场景" :
            isTerrain ? "地形场景" : info.Title;
        _scroll.ContextMenu = isWorld ? _sceneMenu : isTerrain ? _terrainMenu : null;
        _infoTitle.Text = info.LeftTitle;
        _infoDetail.Text = info.LeftDetail;

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
        else
            _scroll.Content = _infoPanel;
    }
}
