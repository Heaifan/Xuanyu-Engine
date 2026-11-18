using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoTerrainFrameCache<(GeoTileId, GeoTerrainEdgeNeighbors), RenderMesh>
        _stitchedMeshes = new();
    private bool _firstStitchLogged;

    private void StitchVisibleTiles()
    {
        GeoTileId[] visible = _visibleMeshes.Keys.ToArray();
        if (_footprint is not null)
        {
            StitchFootprint(visible);
            return;
        }
        foreach (GeoTileId id in visible)
        {
            GeoTerrainEdgeNeighbors neighbors = FindNeighbors(id, visible);
            if (neighbors.IsEmpty)
            {
                _visibleMeshes[id] = _meshes[id]!;
                continue;
            }
            var key = (id, neighbors);
            if (!_stitchedMeshes.TryGetValue(key, out RenderMesh? mesh))
            {
                mesh = BuildRectangularMesh(id, neighbors, false);
                if (mesh is null) continue;
                _stitchedMeshes.Add(key, mesh);
                if (!_firstStitchLogged)
                {
                    _firstStitchLogged = true;
                    Notice?.Invoke($"G-03B LOD 共边对齐启用：{id}");
                }
            }
            _visibleMeshes[id] = mesh;
        }
    }

    private GeoTerrainEdgeNeighbors FindNeighbors(
        GeoTileId current, IEnumerable<GeoTileId> visible)
    {
        GeoTileBounds a = Clip(current);
        GeoTileId? north = null, south = null, west = null, east = null;
        foreach (GeoTileId id in visible)
        {
            if (id.Level >= current.Level) continue;
            GeoTileBounds b = Clip(id);
            bool lon = Math.Min(a.EastLongitudeDegrees, b.EastLongitudeDegrees) >
                Math.Max(a.WestLongitudeDegrees, b.WestLongitudeDegrees);
            bool lat = Math.Min(a.NorthLatitudeDegrees, b.NorthLatitudeDegrees) >
                Math.Max(a.SouthLatitudeDegrees, b.SouthLatitudeDegrees);
            const double epsilon = 1e-9;
            if (lon && Math.Abs(a.NorthLatitudeDegrees - b.SouthLatitudeDegrees) < epsilon)
                north = id;
            if (lon && Math.Abs(a.SouthLatitudeDegrees - b.NorthLatitudeDegrees) < epsilon)
                south = id;
            if (lat && Math.Abs(a.WestLongitudeDegrees - b.EastLongitudeDegrees) < epsilon)
                west = id;
            if (lat && Math.Abs(a.EastLongitudeDegrees - b.WestLongitudeDegrees) < epsilon)
                east = id;
        }
        return new GeoTerrainEdgeNeighbors(north, south, west, east);
    }

    private GeoTileBounds Clip(GeoTileId id)
    {
        GeoTileBounds area = GeographicTileScheme.Bounds(id);
        return new GeoTileBounds(
            Math.Max(area.SouthLatitudeDegrees, _coverage.SouthLatitudeDegrees),
            Math.Max(area.WestLongitudeDegrees, _coverage.WestLongitudeDegrees),
            Math.Min(area.NorthLatitudeDegrees, _coverage.NorthLatitudeDegrees),
            Math.Min(area.EastLongitudeDegrees, _coverage.EastLongitudeDegrees));
    }
}