
namespace XuanYu.World.Maps;

public sealed class NasademTile
{
    public const int Side = 3601;
    public const int SampleCount = Side * Side;
    public const long ByteLength = (long)SampleCount * sizeof(short);

    private readonly short[] _samples;

    public string TileId { get; }
    public int SouthLatitude { get; }
    public int WestLongitude { get; }
    public int NoDataCount { get; }
    public short MinElevation { get; }
    public short MaxElevation { get; }

    internal NasademTile(string id, int latitude, int longitude,
        short[] samples, int noData, short min, short max)
    {
        TileId = id;
        SouthLatitude = latitude;
        WestLongitude = longitude;
        _samples = samples;
        NoDataCount = noData;
        MinElevation = min;
        MaxElevation = max;
    }

    public short? HeightAt(int column, int rowFromSouth)
    {
        short value = _samples[(Side - 1 - rowFromSouth) * Side + column];
        return value == short.MinValue ? null : value;
    }
}
