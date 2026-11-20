namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private static int ContourPrecision(GeoContourStyle style) =>
        style.IntervalMeters switch
        {
            <= 50 => 1,
            <= 100 => 2,
            <= 200 => 5,
            <= 500 => 50,
            _ => 200
        };
}