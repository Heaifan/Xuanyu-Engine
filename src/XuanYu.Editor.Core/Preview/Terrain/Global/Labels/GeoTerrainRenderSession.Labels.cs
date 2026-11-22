namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private static readonly bool R06RawContour =
        Environment.GetEnvironmentVariable("XYE_G06_DEM_CONTOUR") == "1";
    public bool ContourLabelsEnabled { get; private set; } = true;

    public void SetContourLabelsEnabled(bool enabled) => ContourLabelsEnabled = enabled;

    public IReadOnlyList<GeoContourLabel> VisibleContourLabels(uint width, uint height)
    {
        if (!R06RawContour || !ContoursEnabled || !ContourLabelsEnabled ||
            width < 200 || height < 150 || Camera.DistanceMeters > 20_000d) return [];
        GeoContourStyle style = ContourStyle;
        int interval = style.IntervalBlend >= 0.5f
            ? style.NextIntervalMeters : style.IntervalMeters;
        return GeoContourLabelGrid.Build(Camera.Target, Camera.DistanceMeters,
            Math.Max(100, interval * 5), ViewProjection(width, height),
            _anchor, width, height, _source.SampleResident);
    }
}