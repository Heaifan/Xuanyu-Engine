using XuanYu.Core.Geodesy;

namespace XuanYu.World.Maps;

public sealed class GeoUnifiedHeightPage
{
    private const int Count = 33 * 33;
    private readonly GeoUnifiedSample[] _samples;
    private readonly short[] _normals;
    private readonly short[] _geoid;
    internal (Guid, long) Identity { get; }
    public GeoUnifiedTile Tile { get; }
    internal bool NormalsReady { get; }
    public ReadOnlyMemory<short> EncodedNormals => _normals;
    public ReadOnlyMemory<short> GeoidCentimeters => _geoid;

    private GeoUnifiedHeightPage(GeoUnifiedTile tile, GeoUnifiedSample[] samples,
        short[] normals, short[] geoid, bool normalsReady, (Guid, long) identity)
    {
        Tile = tile;
        _samples = samples;
        _normals = normals;
        _geoid = geoid;
        NormalsReady = normalsReady;
        Identity = identity;
    }

    public static GeoUnifiedHeightPage Create(GeoUnifiedTile tile, GeoUnifiedSamples source)
    {
        using var pin = source.PinResident();
        var samples = new GeoUnifiedSample[Count];
        var normals = new short[2 * Count];
        short[] geoid = source.Geoid is null ? [] : new short[Count];
        bool ready = true;
        int step = 1 << tile.Level;
        for (int row = 0; row < 33; row++)
            for (int col = 0; col < 33; col++)
            {
                int i = row * 33 + col;
                var sample = source.Sample(tile.West + col * step, tile.South + row * step, out var point);
                samples[i] = sample;
                // 几何位置使用实际顶点坐标，而非边缘外推所借用的 HGT 样本坐标。
                if (geoid.Length != 0)
                    geoid[i] = source.GeoidCentimetersAt(tile.West + col * step, tile.South + row * step);
                if (sample.Height is not { } height) { ready = false; continue; }
                if (!GeoUnifiedSlope.TryEncode(source, point.X, point.Y, height,
                    out normals[i], out normals[Count + i])) ready = false;
            }
        return new GeoUnifiedHeightPage(tile, samples, normals, geoid, ready, source.Identity);
    }

    public GeoSampleStatus StatusAt(int column, int row) => At(column, row).Status;
    public short? HeightAt(int column, int row) => At(column, row).Height;

    public EnuPosition PositionAt(EnuFrame anchor, int column, int row)
    {
        short height = HeightAt(column, row)!.Value;
        int step = 1 << Tile.Level;
        short geoid = _geoid.Length == 0 ? short.MinValue : _geoid[row * 33 + column];
        return GeoUnifiedPosition.At(anchor, Tile.West + column * step,
            Tile.South + row * step, height, geoid);
    }

    private GeoUnifiedSample At(int column, int row)
    {
        if ((uint)column >= 33 || (uint)row >= 33) throw new ArgumentOutOfRangeException();
        return _samples[row * 33 + column];
    }
}