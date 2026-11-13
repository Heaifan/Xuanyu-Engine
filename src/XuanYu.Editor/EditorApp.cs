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
            var workbench = new Workbench(use3D: true);
            try
            {
                _gpu = new VkContext();
                workbench.BindGpu(_gpu.Gpu);
            }
            catch (Exception exception)
            {
                workbench.ReportGpuFailure(exception.Message);
            }
            bool probe = OperatingSystem.IsWindows() && _gpu is not null &&
                Environment.GetEnvironmentVariable("XYE_E306_GPU_PROBE") == "1";
            desktop.MainWindow = probe
                ? new WinSharedProbeWindow(_gpu!, workbench)
                : new EditorWindow(workbench, _gpu);
            desktop.Exit += (_, _) => _gpu?.Dispose();
        }
        base.OnFrameworkInitializationCompleted();
    }
}