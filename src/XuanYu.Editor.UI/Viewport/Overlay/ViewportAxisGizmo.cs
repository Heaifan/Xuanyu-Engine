using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportAxisGizmo : Canvas
{
    private static readonly Vector3[] Axes =
        [Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY,
         -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ];
    private static readonly string[] Tints =
        ["#D45465", "#A34150", "#84B64B", "#557E37", "#4085D8", "#295786"];
    private readonly Border[] _dots = new Border[6];
    private readonly Line[] _lines = new Line[6];
    private readonly DispatcherTimer _timer = new()
        { Interval = TimeSpan.FromMilliseconds(32) };
    private Func<ViewportCamera?>? _camera;

    public ViewportAxisGizmo()
    {
        Width = 76; Height = 94; Background = Brushes.Transparent;
        var ring = new Ellipse
        {
            Width = 68, Height = 68,
            Fill = new SolidColorBrush(Color.Parse("#42556B"), 0.10),
            Stroke = new SolidColorBrush(Color.Parse("#728398"), 0.35),
            StrokeThickness = 1, IsHitTestVisible = false
        };
        SetLeft(ring, 4); SetTop(ring, 9); Children.Add(ring);
        for (int i = 0; i < 6; i++)
        {
            _lines[i] = new Line
            {
                Stroke = Paint(Tints[i]), StrokeThickness = 1.5,
                IsHitTestVisible = false, ZIndex = 1
            };
            Children.Add(_lines[i]);
            int axis = i;
            var dot = new Border
            {
                Width = 20, Height = 20, CornerRadius = new CornerRadius(10),
                Background = Paint(Tints[i]), BorderBrush = Brushes.White,
                BorderThickness = new Thickness(0.6),
                Child = new TextBlock
                {
                    Text = i % 2 == 0 ? "XYZ"[i / 2].ToString() : "",
                    Foreground = Brushes.White, FontSize = 10,
                    FontWeight = FontWeight.SemiBold,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                }
            };
            ToolTip.SetTip(dot,
                $"{(i % 2 == 0 ? "+" : "−")}{"XYZ"[i / 2]} 视图 · 拖动旋转");
            dot.PointerPressed += (_, e) => BeginOrbit(e, axis);
            _dots[i] = dot; Children.Add(dot);
        }
        ToolTip.SetTip(this, "拖动旋转视角；单击轴球切换方向");
        PointerPressed += (_, e) => BeginOrbit(e, -1);
        PointerMoved += (_, e) => MoveOrbit(e);
        PointerReleased += (_, e) => EndOrbit(e);
        PointerCaptureLost += (_, _) => _pressedAxis = -2;
        _timer.Tick += (_, _) => Refresh();
        AttachedToVisualTree += (_, _) => _timer.Start();
        DetachedFromVisualTree += (_, _) => _timer.Stop();
    }

    public void Bind(Func<ViewportCamera?> camera)
    {
        _camera = camera;
        Refresh();
    }

    private static IBrush Paint(string hex) =>
        new SolidColorBrush(Color.Parse(hex));
}