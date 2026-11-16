namespace XuanYu.World.Maps;

public sealed partial class NasademElevationSource
{
    public double? Sample(double latitudeDegrees, double longitudeDegrees)
    {
        if (!double.IsFinite(latitudeDegrees) ||
            latitudeDegrees is < -90d or > 90d)
            throw new ArgumentOutOfRangeException(nameof(latitudeDegrees));
        if (!double.IsFinite(longitudeDegrees) ||
            longitudeDegrees is < -180d or > 180d)
            throw new ArgumentOutOfRangeException(nameof(longitudeDegrees));

        double longitude = longitudeDegrees == 180d ? -180d : longitudeDegrees;
        int south = Math.Min(89, (int)Math.Floor(latitudeDegrees));
        int west = (int)Math.Floor(longitude);
        int previousWest = west == -180 ? 179 : west - 1;
        bool onSouthEdge = latitudeDegrees == south && south > -90;
        bool onWestEdge = longitude == west;

        if (_tiles.TryGetValue((south, west), out NasademTile? tile))
            return Read(tile, latitudeDegrees, longitude);
        if (onSouthEdge && _tiles.TryGetValue((south - 1, west), out tile))
            return Read(tile, latitudeDegrees, longitude);
        if (onWestEdge && _tiles.TryGetValue((south, previousWest), out tile))
            return Read(tile, latitudeDegrees, longitude);
        if (onSouthEdge && onWestEdge &&
            _tiles.TryGetValue((south - 1, previousWest), out tile))
            return Read(tile, latitudeDegrees, longitude);
        return null;
    }

    private static double? Read(NasademTile tile, double latitude,
        double longitude) => Interpolate(tile, latitude,
            longitude < tile.WestLongitude ? longitude + 360d : longitude);
}