using System.Buffers.Binary;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

internal sealed class G02BHgtFixture : IDisposable
{
    public string Path { get; }

    public G02BHgtFixture(string tileId)
    {
        Path = System.IO.Path.Combine(AppContext.BaseDirectory,
            $"XYE-G02B-{Guid.NewGuid():N}_{tileId}.hgt");
        using var file = File.Create(Path);
        file.SetLength(NasademTile.ByteLength);
    }

    public void SetHeight(int column, int rowFromSouth, short height)
    {
        using var file = File.OpenWrite(Path);
        file.Position = ((long)(NasademTile.Side - 1 - rowFromSouth) *
            NasademTile.Side + column) * sizeof(short);
        Span<byte> bytes = stackalloc byte[2];
        BinaryPrimitives.WriteInt16BigEndian(bytes, height);
        file.Write(bytes);
    }

    public NasademTile Load() => NasademHgtFile.Load(Path);

    public void Dispose() => File.Delete(Path);
}