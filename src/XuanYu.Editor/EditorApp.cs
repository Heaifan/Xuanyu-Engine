using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI;

namespace XuanYu.Editor;

/// <summary>
/// 编辑器 Avalonia 应用程序宿主。
/// </summary>
public sealed class EditorApp : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Dark;
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new EditorWindow(new Workbench());
        }

        base.OnFrameworkInitializationCompleted();
    }
}