using Avalonia.Controls;
using XuanYu.Editor.UI.Top.Terrain;

namespace XuanYu.Editor.UI.Left;

internal sealed partial class LeftTerrainPanel
{
    internal ContextMenu CreateTerrainMenu()
    {
        ContextMenu menu = LeftMenuStyle.Create();
        MenuItem add = LeftMenuStyle.Item("添加", "＋");
        MenuItem import = LeftMenuStyle.Item("导入 NASADEM…", "▧");
        import.Click += async (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is Window owner)
            {
                await TerrainFileMenu.Import(owner, _wb);
                Refresh();
            }
        };
        add.Items.Add(import);
        menu.Items.Add(add);
        return menu;
    }
}
