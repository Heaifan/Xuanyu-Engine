namespace XuanYu.World.Maps;

public sealed class GeoUnifiedHeightPage
{
    private const int Count = 33 * 33;
    private readonly GeoUnifiedSample[] _samples;
    private readonly short[] _normals;
    internal (Guid, long) Identity { get; }
    public GeoUnifiedTile Tile { get; }
    internal bool NormalsReady { get; }
    public ReadOnlyMemory<short> EncodedNormals => _normals;

    private GeoUnifiedHeightPage(GeoUnifiedTile tile, GeoUnifiedSample[] samples,
        short[] normals, bool normalsReady, (Guid, long) identity)
    {
        Tile = tile;
        _samples = samples;
        _normals = normals;
        NormalsReady = normalsReady;
        Identity = identity;
    }

    public static GeoUnifiedHeightPage Create(GeoUnifiedTile tile, GeoUnifiedSamples source)
    {
        using var pin = source.PinResident();
        var samples = new GeoUnifiedSample[Count];
        var normals = new short[2 * Count];
        bool ready = true;
        int step = 1 << tile.Level;
        for (int row = 0; row < 33; row++)
            for (int col = 0; col < 33; col++)
            {
                int i = row * 33 + col;
                var sample = source.Sample(tile.West + col * step, tile.South + row * step, out var point);
                samples[i] = sample;
                if (sample.Height is not { } height) { ready = false; continue; }
                if (!GeoUnifiedSlope.TryEncode(source, point.X, point.Y, height,
                    out normals[i], out normals[Count + i])) ready = false;
            }
        return new GeoUnifiedHeightPage(tile, samples, normals, ready, source.Identity);
    }

    public GeoSampleStatus StatusAt(int column, int row) => At(column, row).Status;
    public short? HeightAt(int column, int row) => At(column, row).Height;

    private GeoUnifiedSample At(int column, int row)
    {
        if ((uint)column >= 33 || (uint)row >= 33) throw new ArgumentOutOfRangeException();
        return _samples[row * 33 + column];
    }
}