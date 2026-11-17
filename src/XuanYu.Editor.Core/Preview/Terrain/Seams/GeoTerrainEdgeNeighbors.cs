using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoTerrainEdgeNeighbors(
    GeoTileId? North, GeoTileId? South,
    GeoTileId? West, GeoTileId? East)
{
    public bool IsEmpty => North is null && South is null &&
        West is null && East is null;
}