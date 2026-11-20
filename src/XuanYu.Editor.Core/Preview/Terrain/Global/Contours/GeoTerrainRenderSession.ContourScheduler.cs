using System.Diagnostics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private EcefPosition _lastContourEye;
    private bool _hasContourEye;
    private int _stillContourFrames;

    private void AdvanceContourBuild(GeoContourStyle style)
    {
        EcefPosition eye = Camera.PositionEcef;
        bool moving = !_hasContourEye || eye != _lastContourEye;
        _hasContourEye = true;
        _lastContourEye = eye;
        _stillContourFrames = moving ? 0 : Math.Min(_stillContourFrames + 1, 4);
        double budgetMs = _stillContourFrames >= 3 ? 12d : 2d;
        var watch = Stopwatch.StartNew();
        do
        {
            if (_contourBuild is null)
            {
                GeoTileId? next = _visibleMeshes.Keys
                    .Where(id => _visibleMeshes[id].TerrainElevations.IsEmpty)
                    .Where(id => style.OpacityAtLod(id.Level) > 0f)
                    .Where(id => !_contourMeshes.TryGetValue(id, out var entry) ||
                        entry.Style != style)
                    .OrderBy(id => _contourMeshes.ContainsKey(id) ? 1 : 0)
                    .ThenBy(DistanceFromTarget)
                    .Select(id => (GeoTileId?)id).FirstOrDefault();
                if (next is not { } id) break;
                bool useRaw = Camera.DistanceMeters <= 20_000d &&
                    (!StreamingEnabled || _source.PrepareNativeRegion(
                        GeographicTileScheme.Bounds(id)));
                NasademElevationSource sampler = useRaw
                    ? _source : _source.CoarseFor(id.Level);
                RenderMesh surface = _visibleMeshes[id];
                _contourBuildingSurface = surface;
                _contourBuild = new GeoContourElevationTask(
                    id, _coverage, sampler, _anchor, style, ContourPrecision(style));
            }
            if (_contourBuild.Advance(1))
            {
                GeoTileId id = _contourBuild.TileId;
                RenderMesh surface = _contourBuildingSurface!;
                Action<string>? report = _g06DiagnosticEnabled ?
                    message => Notice?.Invoke(message) : null;
                _contourMeshes[id] = (surface, _contourBuild.Finish(surface, report), style);
                _contourBuild = null;
                _contourBuildingSurface = null;
            }
        }
        while (watch.Elapsed.TotalMilliseconds < budgetMs);
    }
}
