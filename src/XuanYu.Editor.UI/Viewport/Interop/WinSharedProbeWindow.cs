using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using XuanYu.Render.Vulkan;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class WinSharedProbeWindow : Window
{
    private readonly VkContext _vk;
    private readonly Workbench _workbench;
    private readonly ViewportCompositionSurface _scene = new();
    private readonly TextBlock _status = new()
        { Foreground = Brushes.White, FontSize = 13 };
    private readonly DispatcherTimer _timer = new()
        { Interval = TimeSpan.FromMilliseconds(16) };
    private ICompositionGpuInterop? _interop;
    private ICompositionImportedGpuImage? _image;
    private WinSharedTexture? _texture;
    private VkSharedColorProbe? _probe;
    private Task _frame = Task.CompletedTask;
    private DateTime _resizeAt;

    public WinSharedProbeWindow(VkContext vk, Workbench workbench)
    {
        _vk = vk;
        _workbench = workbench;
        Title = "XYE E3-06C - 真实 3D / MSAA 4x / Avalonia 合成";
        Width = 920; Height = 650;
        var root = new Grid { Background = Brushes.Black };
        root.Children.Add(_scene);
        var navigation = new ViewportNavigationPanel
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Margin = new Thickness(0, 12, 12, 0)
        };
        navigation.Bind(workbench);
        root.Children.Add(navigation);
        root.Children.Add(new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(30, 40, 55), 0.9),
            Padding = new Thickness(12), Margin = new Thickness(12),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
            Child = _status
        });
        Content = root;
        _status.Text = "等待视口布局与 Vulkan GPU 合成...";
        _scene.GpuInteropReady += interop =>
        {
            _interop = interop;
            _timer.Start();
        };
        _scene.SizeChanged += (_, _) => _resizeAt = DateTime.UtcNow;
        _timer.Tick += (_, _) =>
        {
            if (_frame.IsCompleted) _frame = RenderAsync();
        };
        Closed += async (_, _) =>
        {
            _timer.Stop();
            await _frame;
            await ReleaseResourcesAsync();
        };
    }
}