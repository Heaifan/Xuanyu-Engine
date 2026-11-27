using System.Diagnostics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private long _lastLabelGapReport = -60;

    public RenderDraw[] AttachContourLabelGaps(IReadOnlyList<RenderDraw> draws,
        IReadOnlyList<GeoContourLabel> anchors, uint width, uint height, float scale,
        out GeoContourLabel[] visible, Func<int, float>? measure = null)
    {
        if (!ContoursEnabled || !ContourLabelsEnabled || anchors.Count == 0)
        {
            visible = [];
            return draws.ToArray();
        }
        long start = _r02PerfEnabled ? Stopwatch.GetTimestamp() : 0;
        var result = ContourLabelGapLayout.Attach(draws, anchors, ViewProjection(width, height),
            _labelVisibility, CanOwn, width, height, scale, out visible, measure);
        if (_r02PerfEnabled && _monitorFrame - _lastLabelGapReport >= 60)
        {
            _lastLabelGapReport = _monitorFrame;
            Notice?.Invoke($"[G06/R08-03B/标高布局] 帧={_monitorFrame} 候选={anchors.Count} 可见={visible.Length} 耗时={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1}ms");
        }
        return result;
    }
}