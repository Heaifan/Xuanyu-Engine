using System.Numerics;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private IPointer? _sculptPointer;
    private DispatcherTimer? _sculptTimer;
    private Point _sculptPosition;
    private readonly List<Point> _sculptMoves = [];
    private long _sculptTick;

    private bool TryHandleSculptPress(PointerPressedEventArgs e)
    {
        if (_sculptPointer is not null) return true;
        var buttons = e.GetCurrentPoint(this).Properties;
        if (_dragging || _panMode || buttons.IsMiddleButtonPressed ||
            !buttons.IsLeftButtonPressed || !_wb.CanPreviewTerrainBrush) return false;
        Focus();
        ToolTip.SetIsOpen(this, false);
        Point point = e.GetPosition(this);
        var (pixel, width, height) = SculptPixel(point);
        if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
        {
            _wb.TrySampleTerrainElevation(pixel, width, height);
            return true;
        }
        if (_wb.TryBeginTerrainSculpt(pixel, width, height,
            e.KeyModifiers.HasFlag(KeyModifiers.Control)))
        {
            _sculptPointer = e.Pointer;
            e.Pointer.Capture(this);
            _sculptPosition = point;
            _sculptTick = Stopwatch.GetTimestamp();
            _sculptMoves.Clear();
            _sculptTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
            _sculptTimer.Tick += OnSculptTick;
            _sculptTimer.Start();
        }
        return true;
    }

    private bool TryHandleSculptMove(PointerEventArgs e)
    {
        if (_sculptPointer is null) return false;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ||
            !_wb.CanPreviewTerrainBrush)
        {
            FinishSculptInput();
            return true;
        }
        QueueSculptPosition(e.GetPosition(this));
        return true;
    }

    private bool TryHandleSculptRelease(PointerReleasedEventArgs e)
    {
        if (_sculptPointer is null) return false;
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (_wb.CanPreviewTerrainBrush)
            {
                QueueSculptPosition(e.GetPosition(this));
                DrainSculptInput();
            }
            FinishSculptInput();
        }
        return true;
    }

    private void FinishSculptInput(bool releaseCapture = true)
    {
        if (_sculptTimer is { } timer)
        {
            timer.Stop();
            timer.Tick -= OnSculptTick;
        }
        var pointer = _sculptPointer;
        _sculptPointer = null;
        _sculptMoves.Clear();
        _wb.EndTerrainSculpt();
        if (releaseCapture && pointer?.Captured == this) pointer.Capture(null);
    }

    private (Vector2 Pixel, uint Width, uint Height) SculptPixel(Point point)
    {
        var (width, height) = PixelSize;
        var pixel = new Vector2((float)(point.X * width / Math.Max(1d, Bounds.Width)),
            (float)(point.Y * height / Math.Max(1d, Bounds.Height)));
        return (pixel, width, height);
    }
}