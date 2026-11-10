
using Avalonia.Controls;

namespace XuanYu.Editor.UI;

internal static class TopMenuBar
{
    public static Menu Create(Window owner)
    {
        var exit = new MenuItem
        {
            Header = "退出"
        };
        exit.Click += (_, _) => owner.Close();

        var reset = new MenuItem
        {
            Header = "恢复窗口尺寸"
        };
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
                    ItemsSource = new object[]
                    {
                        new MenuItem
                        {
                            Header = "新建项目",
                            IsEnabled = false
                        },
                        new MenuItem
                        {
                            Header = "打开项目",
                            IsEnabled = false
                        },
                        new MenuItem
                        {
                            Header = "保存项目",
                            IsEnabled = false
                        },
                        new Separator(),
                        exit
                    }
                },
                new()
                {
                    Header = "编辑",
                    ItemsSource = new MenuItem[]
                    {
                        new()
                        {
                            Header = "撤销",
                            IsEnabled = false
                        },
                        new()
                        {
                            Header = "重做",
                            IsEnabled = false
                        }
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
                    ItemsSource = new MenuItem[]
                    {
                        new()
                        {
                            Header = "开始模拟",
                            IsEnabled = false
                        },
                        new()
                        {
                            Header = "停止模拟",
                            IsEnabled = false
                        }
                    }
                }
            }
        };
    }
}
