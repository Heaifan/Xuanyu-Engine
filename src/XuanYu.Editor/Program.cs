using Avalonia;

namespace XuanYu.Editor;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<EditorApp>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}