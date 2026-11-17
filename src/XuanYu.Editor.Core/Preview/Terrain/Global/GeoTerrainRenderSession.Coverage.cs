using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly Dictionary<GeoTileId, RenderMesh> _visibleMeshes = [];
    private bool _firstRefinementLogged;
    public IReadOnlyCollection<GeoTileId> VisibleTileIds => _visibleMeshes.Keys;

    private void UpdateVisibleCover(GeoTerrainFrame frame)
    {
        GeoTileId[] targets = frame.Tiles.Select(tile => tile.RequestedId).ToArray();
        if (targets.Length == 0)
        {
            _visibleMeshes.Clear();
            return;
        }

        foreach (GeoTileId id in _visibleMeshes.Keys.ToArray())
            if (!targets.Any(target => Related(id, target)))
                _visibleMeshes.Remove(id);

        foreach (GeoTileId target in targets)
        {
            RenderMesh? mesh = MeshFor(target);
            if (mesh is null || _visibleMeshes.Keys.Any(id => Contains(id, target)))
                continue;
            foreach (GeoTileId id in _visibleMeshes.Keys.Where(id =>
                Contains(target, id)).ToArray())
                _visibleMeshes.Remove(id);
            _visibleMeshes[target] = mesh;
        }

        foreach (GeoTileId parent in _visibleMeshes.Keys.ToArray())
        {
            if (!targets.Any(target => target.Level > parent.Level &&
                Contains(parent, target))) continue;
            var children = GeographicTileScheme.Children(parent)
                .Where(InCoverage).ToArray();
            var prepared = children.Select(id => (Id: id, Mesh: MeshFor(id))).ToArray();
            if (prepared.Any(item => item.Mesh is null)) continue;
            _visibleMeshes.Remove(parent);
            foreach (var item in prepared)
                _visibleMeshes.Add(item.Id, item.Mesh!);
            if (!_firstRefinementLogged)
            {
                _firstRefinementLogged = true;
                Notice?.Invoke($"G-02F LOD 局部替换：父片 {parent}，子片 {prepared.Length} 片");
            }
        }
    }

    private RenderMesh? MeshFor(GeoTileId id)
    {
        if (_meshes.TryGetValue(id, out RenderMesh? mesh)) return mesh;
        GeoHeightTile? tile = _stream.RequestExact(id);
        if (tile is null) return null;
        mesh = GeoTerrainMeshBuilder.Build(tile, _anchor, _source, _coverage);
        _meshes.Add(id, mesh);
        return mesh;
    }

    private bool InCoverage(GeoTileId id)
    {
        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        return bounds.SouthLatitudeDegrees < _coverage.NorthLatitudeDegrees &&
            bounds.NorthLatitudeDegrees > _coverage.SouthLatitudeDegrees &&
            bounds.WestLongitudeDegrees < _coverage.EastLongitudeDegrees &&
            bounds.EastLongitudeDegrees > _coverage.WestLongitudeDegrees;
    }

    private static bool Contains(GeoTileId outer, GeoTileId inner)
    {
        if (outer.Level > inner.Level) return false;
        int shift = inner.Level - outer.Level;
        return inner.X >> shift == outer.X && inner.Y >> shift == outer.Y;
    }

    private static bool Related(GeoTileId a, GeoTileId b) =>
        Contains(a, b) || Contains(b, a);
}