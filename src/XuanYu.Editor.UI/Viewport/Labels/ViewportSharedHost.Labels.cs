using Avalonia.Controls;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private readonly GeoContourLabelCanvas _contourLabels = new();
    private IReadOnlyList<GeoContourLabel> _worldLabels = [];
    private GeoContourLabel[] _screenLabels = [];
    private long _lastContourLabelsAt;
    private bool _firstLabelsLogged;

    private RenderDraw[] PrepareContourLabels(
        IReadOnlyList<RenderDraw> draws, uint width, uint height)
    {
        float scale = (float)(TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d);
        long now = Environment.TickCount64;
        if (now - _lastContourLabelsAt >= 180)
        {
            _lastContourLabelsAt = now;
            _wb.AttachTerrainContourGaps(draws, _worldLabels, width, height, scale,
                out var retained, _contourLabels.MeasureWidth);
            var candidates = _wb.TerrainContourLabels(width, height);
            var fresh = _wb.BindTerrainContourLabels(draws, candidates, width, height);
            _worldLabels = retained.Concat(fresh.Where(label =>
                !retained.Any(old => ReferenceEquals(old.Owner, label.Owner) ||
                    old.ElevationMeters == label.ElevationMeters))).ToArray();
        }
        var result = _wb.AttachTerrainContourGaps(draws, _worldLabels, width, height,
            scale, out _screenLabels, _contourLabels.MeasureWidth);
        _worldLabels = _screenLabels;
        return result;
    }

    private void DisplayContourLabels()
    {
        double scale = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        _contourLabels.Show(_screenLabels, scale);
        if (!_firstLabelsLogged && _screenLabels.Length > 0)
        {
            _firstLabelsLogged = true;
            _wb.Log.Write($"[G06/R06B-F03] 真实等高线内嵌标高={_screenLabels.Length}");
        }
    }
}