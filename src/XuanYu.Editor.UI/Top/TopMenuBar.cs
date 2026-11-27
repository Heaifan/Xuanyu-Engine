using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI.Top.Persistence;
using XuanYu.Editor.UI.Top.Simulation;
using XuanYu.Editor.UI.Top.Terrain;

namespace XuanYu.Editor.UI;

internal static class TopMenuBar
{
    public static Menu Create(Window owner, Workbench wb)
    {
        var reset = new MenuItem { Header = "恢复窗口尺寸" };
        reset.Click += (_, _) =>
        {
            owner.Width = 1280;
            owner.Height = 800;
        };
        return new Menu
        {
            Background = EditorTheme.PanelBg,
            Foreground = EditorTheme.TextMain,
            ItemsSource = new MenuItem[]
            {
                new() { Header = "文件", ItemsSource = SceneFileMenu.Create(owner, wb) },
                new() { Header = "地图", ItemsSource = TerrainFileMenu.Create(owner, wb) },
                new()
                {
                    Header = "编辑",
                    ItemsSource = new MenuItem[]
                    {
                        CreateHistoryItem(wb, "撤销地形", wb.UndoTerrain, () => wb.CanUndoTerrain),
                        CreateHistoryItem(wb, "重做地形", wb.RedoTerrain, () => wb.CanRedoTerrain)
                    }
                },
                new()
                {
                    Header = "视图",
                    ItemsSource = new MenuItem[]
                    {
                        reset, GeoOverlayViewMenu.Create(wb),
                        ViewportEnvironmentViewMenu.Create(wb)
                    }
                },
                new() { Header = "运行", ItemsSource = SimulationMenu.Create(owner, wb) }
            }
        };
    }
    private static MenuItem CreateHistoryItem(Workbench wb, string title, Action run, Func<bool> canRun)
    {
        var item = new MenuItem { Header = title, IsEnabled = canRun() };
        item.Click += (_, _) => run();
        wb.Changed += () => item.IsEnabled = canRun();
        return item;
    }
}