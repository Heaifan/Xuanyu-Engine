using System.Diagnostics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private EcefPosition _lastContourEye;
    private bool _hasContourEye;
    private int _stillContourFrames;

    private void AdvanceContourBuild(GeoContourStyle style, int step)
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
                    .Where(id => !_contourMeshes.ContainsKey(id))
                    .OrderBy(DistanceFromTarget)
                    .Select(id => (GeoTileId?)id).FirstOrDefault();
                if (next is not { } id) break;
                bool useRaw = Camera.DistanceMeters <= 20_000d &&
                    (!StreamingEnabled || _source.PrepareNativeRegion(
                        GeographicTileScheme.Bounds(id)));
                NasademElevationSource sampler = useRaw
                    ? _source : _source.CoarseFor(id.Level);
                _contourBuild = new GeoContourElevationTask(
                    id, _coverage, sampler, _anchor, style, step);
            }
            if (_contourBuild.Advance(1))
            {
                _contourMeshes.Add(_contourBuild.TileId, _contourBuild.Finish());
                _contourBuild = null;
            }
        }
        while (watch.Elapsed.TotalMilliseconds < budgetMs);
    }
}