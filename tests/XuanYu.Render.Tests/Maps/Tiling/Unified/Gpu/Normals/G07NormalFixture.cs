using System.Buffers.Binary;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

internal sealed class G07NormalFixture : IDisposable
{
    private readonly string _folder = Path.Combine("artifacts", "tests", "G07Normals-" + Guid.NewGuid());
    public NasademTileCatalog Catalog { get; }
    public static GeoUnifiedTile Fine => new(0, 33861, 12736);
    public static GeoUnifiedTile Right => new(0, 33862, 12736);
    public static GeoUnifiedTile Coarse => new(1, 16931, 6368);

    public G07NormalFixture()
    {
        Directory.CreateDirectory(_folder);
        try
        {
            string west = WriteTile(120), east = WriteTile(121);
            Catalog = new NasademTileCatalog([west, east]);
        }
        catch { Dispose(); throw; }
    }

    private string WriteTile(int longitude)
    {
        string path = Path.Combine(_folder, $"n23e{longitude}.hgt");
        using var stream = File.Create(path);
        stream.SetLength(NasademTile.ByteLength);
        int offset = (longitude - 121) * 3600;
        int lo = Math.Max(0, -80 - offset), hi = Math.Min(3600, 80 - offset);
        var bytes = new byte[(hi - lo + 1) * 2];
        for (int y = 720; y <= 840; y++)
        {
            for (int x = lo; x <= hi; x++)
            {
                int global = offset + x;
                int h = 6000 - 100 * global;
                if (global == -15 && y == 768) h += 40;
                BinaryPrimitives.WriteInt16BigEndian(bytes.AsSpan((x - lo) * 2), (short)h);
            }
            stream.Position = ((long)(3600 - y) * 3601 + lo) * 2;
            stream.Write(bytes);
        }
        return path;
    }

    public void Dispose() => Directory.Delete(_folder, true);
}