using System.Diagnostics;
using Avalonia;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private void OnSculptTick(object? sender, EventArgs e)
    {
        if (_sculptPointer is null || !_wb.CanPreviewTerrainBrush)
        {
            FinishSculptInput();
            return;
        }
        DrainSculptInput();
    }

    private void QueueSculptPosition(Point point)
    {
        if (point == _sculptPosition) return;
        _sculptPosition = point;
        _sculptMoves.Add(point);
    }

    private void DrainSculptInput()
    {
        long now = Stopwatch.GetTimestamp();
        float seconds = Math.Clamp((float)Stopwatch.GetElapsedTime(_sculptTick, now).TotalSeconds, 0f, 0.1f);
        _sculptTick = now;
        foreach (Point point in _sculptMoves)
        {
            var (pixel, width, height) = SculptPixel(point);
            _wb.MoveTerrainSculpt(pixel, width, height);
        }
        _sculptMoves.Clear();
        _wb.AdvanceTerrainSculpt(seconds);
    }
}