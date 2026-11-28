using System.Buffers.Binary;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

internal sealed class E602CHgtFixture : IDisposable
{
    private readonly string _folder = Path.Combine(Environment.CurrentDirectory,
        "artifacts", "tests", Guid.NewGuid().ToString("N"));

    public string Create(string tile, params (int X, int Y, short Height)[] samples)
    {
        Directory.CreateDirectory(_folder);
        string path = Path.Combine(_folder, tile + ".hgt");
        using var stream = File.Create(path);
        stream.SetLength(NasademTile.ByteLength);
        Span<byte> bytes = stackalloc byte[2];
        foreach (var (x, y, height) in samples)
        {
            stream.Position = ((long)(3600 - y) * NasademTile.Side + x) * 2;
            BinaryPrimitives.WriteInt16BigEndian(bytes, height);
            stream.Write(bytes);
        }
        return path;
    }

    public void Dispose() => Directory.Delete(_folder, true);
}