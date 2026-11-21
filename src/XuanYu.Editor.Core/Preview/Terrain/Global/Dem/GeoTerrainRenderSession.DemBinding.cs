using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private void BindResidentDem(RenderDraw[] draws)
    {
        if (Camera.DistanceMeters > 20_000d) return;
        Dictionary<RenderMesh, GeoTileId> owners = VisibleDemOwners();
        int bound = 0;
        for (int i = 0; i < draws.Length; i++)
        {
            if (!owners.TryGetValue(draws[i].Mesh, out GeoTileId id)) continue;
            if (ReadResidentDem(id) is not { } dem) continue;
            draws[i] = draws[i] with { ResidentDem = dem };
            bound++;
        }
        if (_g06DiagnosticEnabled && _monitorFrame % 120 == 0)
            Notice?.Invoke($"[G06/DEM通道] 已附加={bound}/{draws.Length} " +
                "（仅 CPU 数据，尚未启用 Vulkan 高程纹理）");
    }

    private Dictionary<RenderMesh, GeoTileId> VisibleDemOwners()
    {
        var owners = new Dictionary<RenderMesh, GeoTileId>();
        foreach (var (id, mesh) in _visibleMeshes)
        {
            owners[mesh] = id;
            if (!_selectionMeshes.TryGetValue(mesh, out var parts)) continue;
            if (parts.Inner is not null) owners[parts.Inner] = id;
            if (parts.Outer is not null) owners[parts.Outer] = id;
        }
        return owners;
    }

    private RenderDemTile? ReadResidentDem(GeoTileId id)
    {
        GeoTileBounds b = GeographicTileScheme.Bounds(id);
        int south = (int)Math.Floor(b.SouthLatitudeDegrees);
        int north = (int)Math.Ceiling(b.NorthLatitudeDegrees) - 1;
        int west = (int)Math.Floor(b.WestLongitudeDegrees);
        int east = (int)Math.Ceiling(b.EastLongitudeDegrees) - 1;
        if (south != north || west != east) return null;
        NasademTile? tile = _source.TryGetResidentTile(south, west);
        return tile is null ? null : new RenderDemTile(
            tile.SouthLatitude, tile.WestLongitude, tile.SamplesNorthToSouth);
    }
}