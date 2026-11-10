
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using XuanYu.Editor.Pages;
using XuanYu.Editor.UI;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor;

public sealed class EditorApp : Application
{
    private VkContext? _gpu;

    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Light;
        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var workbench = new Workbench();
            try
            {
                _gpu = new VkContext();
                workbench.BindGpu(_gpu.Gpu);
            }
            catch (Exception exception)
            {
                workbench.ReportGpuFailure(exception.Message);
            }
            desktop.MainWindow = new EditorWindow(workbench, _gpu);
            desktop.Exit += (_, _) => _gpu?.Dispose();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
