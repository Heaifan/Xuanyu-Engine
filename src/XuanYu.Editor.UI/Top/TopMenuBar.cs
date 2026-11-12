using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI.Top.Persistence;
using XuanYu.Editor.UI.Top.Simulation;

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
                new()
                {
                    Header = "文件",
                    ItemsSource = SceneFileMenu.Create(owner, wb)
                },
                new()
                {
                    Header = "编辑",
                    ItemsSource = new MenuItem[]
                    {
                        new() { Header = "撤销", IsEnabled = false },
                        new() { Header = "重做", IsEnabled = false }
                    }
                },
                new()
                {
                    Header = "视图",
                    ItemsSource = new MenuItem[] { reset }
                },
                new()
                {
                    Header = "运行",
                    ItemsSource = SimulationMenu.Create(owner, wb)
                }
            }
        };
    }
}