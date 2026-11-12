using Avalonia.Controls;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI.Top.Persistence;

internal static class SceneFileMenu
{
    public static object[] Create(Window owner, Workbench wb)
    {
        var commands = new SceneFileCommands(owner, wb);
        var open = new MenuItem { Header = "打开场景..." };
        var save = new MenuItem { Header = "保存场景" };
        var saveAs = new MenuItem { Header = "场景另存为..." };
        var exit = new MenuItem { Header = "退出" };
        open.Click += async (_, _) => await commands.OpenAsync();
        save.Click += async (_, _) => await commands.SaveAsync();
        saveAs.Click += async (_, _) => await commands.SaveAsAsync();
        exit.Click += (_, _) => owner.Close();
        return [
            new MenuItem { Header = "新建项目", IsEnabled = false },
            new MenuItem { Header = "打开项目", IsEnabled = false },
            new MenuItem { Header = "保存项目", IsEnabled = false },
            new Separator(), open, save, saveAs, new Separator(), exit
        ];
    }
}