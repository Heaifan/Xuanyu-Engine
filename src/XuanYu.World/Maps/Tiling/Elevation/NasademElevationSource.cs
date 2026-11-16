namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    private readonly Dictionary<(int South, int West), NasademTile> _tiles;

    public NasademElevationSource(params NasademTile[] tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        _tiles = new Dictionary<(int, int), NasademTile>(tiles.Length);
        foreach (NasademTile tile in tiles)
            _tiles.Add((tile.SouthLatitude, tile.WestLongitude), tile);
    }
}