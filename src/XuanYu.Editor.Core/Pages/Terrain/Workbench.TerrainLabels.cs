using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public bool TerrainContourLabelsEnabled =>
        _geoPreview?.ContourLabelsEnabled ?? (HasLocalTerrainContours && _localContourLabelsEnabled);

    public IReadOnlyList<GeoContourLabel> TerrainContourLabels(uint width, uint height) =>
        (!IsSimulating || CurrentId == PageId.Terrain) && _geoPreview is { } geo
            ? geo.VisibleContourLabels(width, height) : [];

    public GeoContourLabel[] BindTerrainContourLabels(
        IReadOnlyList<RenderDraw> draws, IReadOnlyList<GeoContourLabel> anchors,
        uint width, uint height) => _geoPreview is { } geo
            ? geo.BindContourLabelMeshes(draws, anchors, width, height)
            : BuildLocalContourLabels(draws, width, height);

    public RenderDraw[] AttachTerrainContourGaps(
        IReadOnlyList<RenderDraw> draws, IReadOnlyList<GeoContourLabel> anchors,
        uint width, uint height, float scale, out GeoContourLabel[] visible, Func<int, float>? measure = null)
    {
        if (_geoPreview is { } geo && (!IsSimulating || CurrentId == PageId.Terrain))
            return geo.AttachContourLabelGaps(draws, anchors, width, height,
                scale, out visible, measure);
        return AttachLocalContourGaps(draws, anchors, width, height, scale, out visible, measure);
    }

    public void SetTerrainContourLabelsEnabled(bool enabled)
    {
        if (_geoPreview is { } geo) geo.SetContourLabelsEnabled(enabled);
        else if (HasLocalTerrainContours) _localContourLabelsEnabled = enabled;
        Log.Write($"[G06/R06B] 高程数字：{(enabled ? "开启" : "关闭")}");
        Changed?.Invoke();
    }
}