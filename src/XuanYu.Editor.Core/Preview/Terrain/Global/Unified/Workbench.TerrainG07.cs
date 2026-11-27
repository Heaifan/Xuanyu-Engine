using XuanYu.Editor.Preview.Terrain;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private (GeoTerrainRenderSession Source, long Started)? _g07Import;

    private void TrackG07Import(GeoTerrainRenderSession source, long started) =>
        _g07Import = (source, started);

    public void CompleteG07Import(GeoTerrainRenderSession source)
    {
        if (_g07Import is not { } import || !ReferenceEquals(import.Source, source) ||
            !ReferenceEquals(G07PreviewSource, source)) return;
        _g07Import = null;
        Log.Write($"[G07/导入] HGT={source.SourceCount} 首次合成提交={System.Diagnostics.Stopwatch.GetElapsedTime(import.Started).TotalMilliseconds:F1}ms");
    }

    public GeoTerrainRenderSession? G07PreviewSource =>
        IsSimulating ? null : _geoPreview;

    public IReadOnlyList<RenderDraw> G07EnvironmentDraws(GeoTerrainRenderSession source,
        GeoUnifiedLodSession cpu, uint width, uint height)
    {
        var draws = new List<RenderDraw>(2);
        if (SkyEnabled) draws.Add(ViewportSky.WithPitch(source.Camera.PitchRadians));
        bool terrainPage = CurrentId == PageId.Terrain;
        if ((terrainPage ? TerrainGridEnabled : WorldGridEnabled) &&
            source.GridDraw(terrainPage, width, height, cpu) is { } grid)
            draws.Add(grid);
        return draws;
    }
}