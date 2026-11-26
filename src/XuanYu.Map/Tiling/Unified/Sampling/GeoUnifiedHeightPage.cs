namespace XuanYu.World.Maps;

public sealed class GeoUnifiedHeightPage
{
    private readonly GeoUnifiedSample[] _samples;
    internal (Guid, long) Identity { get; }
    public GeoUnifiedTile Tile { get; }

    private GeoUnifiedHeightPage(GeoUnifiedTile tile, GeoUnifiedSample[] samples, (Guid, long) identity)
    {
        Tile = tile;
        _samples = samples;
        Identity = identity;
    }

    public static GeoUnifiedHeightPage Create(GeoUnifiedTile tile, GeoUnifiedSamples source)
    {
        using var pin = source.PinResident();
        var samples = new GeoUnifiedSample[33 * 33];
        int step = 1 << tile.Level;
        for (int row = 0; row < 33; row++)
            for (int col = 0; col < 33; col++)
                samples[row * 33 + col] = source.Sample(
                    tile.West + col * step, tile.South + row * step);
        return new GeoUnifiedHeightPage(tile, samples, source.Identity);
    }

    public GeoSampleStatus StatusAt(int column, int row) => At(column, row).Status;
    public short? HeightAt(int column, int row) => At(column, row).Height;

    private GeoUnifiedSample At(int column, int row)
    {
        if ((uint)column >= 33 || (uint)row >= 33) throw new ArgumentOutOfRangeException();
        return _samples[row * 33 + column];
    }
}
