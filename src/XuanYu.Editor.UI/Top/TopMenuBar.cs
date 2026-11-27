using Avalonia;
using Avalonia.Layout;
using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI.Top.Persistence;
using XuanYu.Editor.UI.Top.Simulation;
using XuanYu.Editor.UI.Top.Terrain;

namespace XuanYu.Editor.UI;

internal static class TopMenuBar
{
    public static Menu Create(Window owner, Workbench wb,
        SceneFileCommands scene, SimulationMenu simulation)
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
                new() { Header = "文件", ItemsSource = SceneFileMenu.Create(owner, wb, scene) },
                new()
                {
                    Header = "编辑",
                    ItemsSource = new MenuItem[]
                    {
                        CreateHistoryItem(wb, "撤销地形", wb.UndoTerrain, () => wb.CanUndoTerrain),
                        CreateHistoryItem(wb, "重做地形", wb.RedoTerrain, () => wb.CanRedoTerrain)
                    }
                },
                new() { Header = "地图", ItemsSource = TerrainFileMenu.Create(owner, wb) },
                new()
                {
                    Header = "视图",
                    ItemsSource = new MenuItem[]
                    {
                        reset, CreateWorkspaceMenu(wb), GeoOverlayViewMenu.Create(wb),
                        ViewportEnvironmentViewMenu.Create(wb)
                    }
                },
                new() { Header = "运行", ItemsSource = simulation.Create() },
                new() { Header = "帮助", ItemsSource = new[] { CreateAbout(owner) } }
            }
        };
    }

    private static MenuItem CreateWorkspaceMenu(Workbench wb)
    {
        var pages = PageCatalog.All.Select(page =>
        {
            var item = new MenuItem { Header = page.Title };
            item.Click += (_, _) => wb.SwitchTo(page.Id);
            return item;
        }).ToArray();
        return new MenuItem { Header = "工作区", ItemsSource = pages };
    }

    private static MenuItem CreateAbout(Window owner)
    {
        var item = new MenuItem { Header = "关于玄域引擎" };
        item.Click += async (_, _) =>
        {
            var dialog = new Window { Title = "关于玄域引擎", Width = 360,
                Height = 180, CanResize = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner };
            var close = new Button { Content = "关闭", HorizontalAlignment = HorizontalAlignment.Right };
            close.Click += (_, _) => dialog.Close();
            dialog.Content = new StackPanel { Margin = new Thickness(16), Spacing = 10,
                Children = { new TextBlock { Text = "XuanYu Engine · Editor", FontSize = 16 },
                    new TextBlock { Text = $"UI 程序集版本：{typeof(EditorWindow).Assembly.GetName().Version}\n.NET：{Environment.Version}" }, close } };
            await dialog.ShowDialog(owner);
        };
        return item;
    }

    private static MenuItem CreateHistoryItem(Workbench wb, string title, Action run, Func<bool> canRun)
    {
        var item = new MenuItem { Header = title, IsEnabled = canRun() };
        item.Click += (_, _) => run();
        wb.Changed += () => item.IsEnabled = canRun();
        return item;
    }
}