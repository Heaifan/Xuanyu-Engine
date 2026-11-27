using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoTerrainFootprintGrid? _footprint;
    private Dictionary<GeoTileId, GeoTerrainFootprintTile> _footprintTiles = [];
    private readonly Dictionary<GeoTileId, (GeoTerrainFootprintTile Tile, RenderMesh Mesh)>
        _footprintMeshes = [];

    private RenderMesh? BuildFootprintBase(GeoTileId id)
    {
        var tile = GeoTerrainFootprintTopology.Build(_footprint!, _source, _anchor, [id])[id];
        return BuildFootprintMesh(tile, false);
    }

    private RenderMesh? BuildFootprintMesh(GeoTerrainFootprintTile tile, bool native) =>
        GeoTerrainFootprintMesh.Build(_footprint!, tile,
            native ? _source : _source.CoarseFor(tile.Id.Level), _anchor, native, _nativeSourceId);

    private void StitchFootprint(GeoTileId[] visible)
    {
        if (visible.Length != _footprintTiles.Count || visible.Any(id => !_footprintTiles.ContainsKey(id)))
            RebuildFootprintTopology(visible);
        foreach (GeoTileId id in visible)
        {
            GeoTerrainFootprintTile tile = _footprintTiles[id];
            if (!tile.Adjusted) continue;
            if (!_footprintMeshes.TryGetValue(id, out var entry) || !ReferenceEquals(entry.Tile, tile))
            {
                RenderMesh? mesh = BuildFootprintMesh(tile, false);
                if (mesh is null) continue;
                entry = (tile, mesh);
                _footprintMeshes[id] = entry;
            }
            _visibleMeshes[id] = entry.Mesh;
        }
    }

    private void RebuildFootprintTopology(GeoTileId[] visible)
    {
        var next = GeoTerrainFootprintTopology.Build(_footprint!, _source, _anchor, visible);
        foreach (GeoTileId id in visible)
            if (_footprintTiles.TryGetValue(id, out var old) && old.Matches(next[id])) next[id] = old;
        _footprintTiles = next;
        foreach (GeoTileId id in _footprintMeshes.Keys.ToArray())
            if (!next.TryGetValue(id, out var tile) || !ReferenceEquals(tile, _footprintMeshes[id].Tile))
                _footprintMeshes.Remove(id);
    }
}