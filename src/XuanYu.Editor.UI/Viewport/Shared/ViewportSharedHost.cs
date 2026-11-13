using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using XuanYu.Editor.Pages;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost : Grid
{
    private readonly VkContext _vk;
    private readonly Workbench _wb;
    private readonly ViewportCompositionSurface _scene = new()
        { IsHitTestVisible = false };
    private readonly DispatcherTimer _timer = new()
        { Interval = TimeSpan.FromMilliseconds(16) };
    private ICompositionGpuInterop? _interop;
    private ICompositionImportedGpuImage? _image;
    private WinSharedTexture? _texture;
    private VkSharedColorProbe? _probe;
    private Task _frame = Task.CompletedTask;
    private DateTime _lastTargetAt;
    private bool _panMode;
    private bool _dragging;
    private bool _panGesture;
    private bool _stopped;
    private Point _previous;
    public bool IsStopped => _stopped;
    public event Action<int, int>? Clicked;

    public (uint Width, uint Height) PixelSize
    {
        get
        {
            double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1;
            return ((uint)Math.Max(0, Math.Round(Bounds.Width * scale)),
                (uint)Math.Max(0, Math.Round(Bounds.Height * scale)));
        }
    }

    public ViewportSharedHost(VkContext vk, Workbench workbench)
    {
        _vk = vk;
        _wb = workbench;
        Background = Brushes.Transparent;
        ClipToBounds = true;
        Focusable = true;
        Children.Add(_scene);
        var navigation = new ViewportNavigationPanel
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 12, 12, 0)
        };
        navigation.Bind(workbench);
        navigation.PanModeChanged += enabled => _panMode = enabled;
        Children.Add(navigation);
        _scene.GpuInteropReady += interop =>
        {
            if (_stopped) return;
            _interop = interop;
            _timer.Start();
        };
        _timer.Tick += (_, _) =>
        {
            if (_frame.IsCompleted) _frame = RenderAsync();
        };
        PointerPressed += OnViewportPointerPressed;
        PointerMoved += OnViewportPointerMoved;
        PointerReleased += OnViewportPointerReleased;
        PointerCaptureLost += (_, _) => _dragging = false;
        PointerWheelChanged += OnViewportWheel;
        KeyDown += OnViewportKeyDown;
        KeyUp += OnViewportKeyUp;
        LostFocus += (_, _) => _wb.SetGameCameraKey(
            XuanYu.Editor.Camera.GameCameraKey.None, false);
    }
}