using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal sealed class GeoTerrainFootprintTile
{
    public GeoTileId Id { get; }
    public GeoTileBounds Bounds { get; }
    // 北、南、西、东；横边从西到东，竖边从南到北。
    public GeoTerrainFootprintEdge[] Edges { get; }
    public bool Adjusted { get; }

    public GeoTerrainFootprintTile(GeoTileId id, GeoTileBounds bounds,
        GeoTerrainFootprintEdge[] edges, bool adjusted)
    {
        Id = id;
        Bounds = bounds;
        Edges = edges;
        Adjusted = adjusted;
    }

    public bool Matches(GeoTerrainFootprintTile other) => Id == other.Id &&
        Adjusted == other.Adjusted && Edges.Zip(other.Edges).All(p => p.First.Matches(p.Second));

    public bool TryBoundary(double latitude, double longitude, out GeoTerrainEdgeVertex? vertex)
    {
        GeoTileBounds b = Bounds;
        vertex = null;
        if (latitude < b.SouthLatitudeDegrees || latitude > b.NorthLatitudeDegrees ||
            longitude < b.WestLongitudeDegrees || longitude > b.EastLongitudeDegrees) return false;
        if (latitude == b.NorthLatitudeDegrees) vertex = Edges[0].At(longitude);
        else if (latitude == b.SouthLatitudeDegrees) vertex = Edges[1].At(longitude);
        else if (longitude == b.WestLongitudeDegrees) vertex = Edges[2].At(latitude);
        else if (longitude == b.EastLongitudeDegrees) vertex = Edges[3].At(latitude);
        else return false;
        return true;
    }

    public bool Touches(GeoTileBounds b, int side)
    {
        GeoTileBounds a = Bounds;
        bool lon = Math.Min(a.EastLongitudeDegrees, b.EastLongitudeDegrees) >
            Math.Max(a.WestLongitudeDegrees, b.WestLongitudeDegrees);
        bool lat = Math.Min(a.NorthLatitudeDegrees, b.NorthLatitudeDegrees) >
            Math.Max(a.SouthLatitudeDegrees, b.SouthLatitudeDegrees);
        return side switch
        {
            0 => lon && a.SouthLatitudeDegrees == b.NorthLatitudeDegrees,
            1 => lon && a.NorthLatitudeDegrees == b.SouthLatitudeDegrees,
            2 => lat && a.EastLongitudeDegrees == b.WestLongitudeDegrees,
            _ => lat && a.WestLongitudeDegrees == b.EastLongitudeDegrees
        };
    }
}