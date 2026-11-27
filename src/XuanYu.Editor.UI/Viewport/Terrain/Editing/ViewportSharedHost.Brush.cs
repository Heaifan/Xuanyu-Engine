using System.Numerics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private readonly TerrainBrushOverlay _brushOverlay = new();
    private Point? _brushPointer;

    private void AttachTerrainBrushOverlay()
    {
        Children.Insert(1, _brushOverlay);
        AddHandler(InputElement.PointerMovedEvent, TrackTerrainBrushPointer,
            RoutingStrategies.Bubble, handledEventsToo: true);
        AddHandler(InputElement.PointerPressedEvent, (_, e) => TrackTerrainBrushPointer(this, e),
            RoutingStrategies.Bubble, handledEventsToo: true);
        PointerExited += (_, _) =>
        {
            FinishBrushAdjust(false);
            ClearTerrainBrushPointer();
        };
        PointerCaptureLost += (_, _) =>
        {
            FinishSculptInput(releaseCapture: false);
            ClearTerrainBrushPointer();
        };
        LostFocus += (_, _) =>
        {
            FinishSculptInput();
            FinishBrushAdjust(false);
        };
        _wb.Changed += () =>
        {
            if (_wb.CanPreviewTerrainBrush) return;
            FinishSculptInput();
            FinishBrushAdjust(false);
            ClearTerrainBrushPointer();
        };
    }

    private void TrackTerrainBrushPointer(object? sender, PointerEventArgs e)
    {
        if (!ReferenceEquals(e.Source, this) || _dragging || _panMode ||
            !_wb.CanPreviewTerrainBrush)
        {
            ClearTerrainBrushPointer();
            return;
        }
        if (_adjustOriginal is null) _brushPointer = e.GetPosition(this);
    }

    private void ClearTerrainBrushPointer()
    {
        _brushPointer = null;
        _brushOverlay.SetFrame(null, default);
    }

    private (TerrainBrushPreview Preview, Matrix4x4 Projection)?
        CaptureTerrainBrushFrame(uint width, uint height)
    {
        if (_brushPointer is not Point p || !_wb.CanPreviewTerrainBrush ||
            _wb.ActiveCamera is not { } camera || Bounds.Width <= 0 || Bounds.Height <= 0)
            return null;
        var pixel = new Vector2((float)(p.X * width / Bounds.Width),
            (float)(p.Y * height / Bounds.Height));
        var preview = _wb.BuildTerrainBrushPreview(pixel, width, height);
        return preview is null ? null : (preview, camera.ViewProjection(width, height));
    }

    private void DisplayTerrainBrushFrame(
        (TerrainBrushPreview Preview, Matrix4x4 Projection)? frame)
    {
        if (_brushPointer is null || !_wb.CanPreviewTerrainBrush || frame is not { } value)
            _brushOverlay.SetFrame(null, default);
        else
            _brushOverlay.SetFrame(value.Preview, value.Projection);
    }
}