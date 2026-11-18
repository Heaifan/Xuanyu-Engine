using System.Buffers.Binary;

namespace XuanYu.World.Maps;

public static partial class XYTerrainReader
{
    public static NasademHgtInfo ReadInfo(string path)
    {
        using var stream = File.OpenRead(path);
        return XYTerrainFormat.Read(stream).Info;
    }

    public static short? ReadHeightAt(string path, int column, int rowFromSouth)
    {
        if ((uint)column >= NasademTile.Side || (uint)rowFromSouth >= NasademTile.Side)
            throw new ArgumentOutOfRangeException(nameof(column));
        using var stream = File.OpenRead(path);
        XYTerrainManifest manifest = XYTerrainFormat.Read(stream);
        int row = NasademTile.Side - 1 - rowFromSouth;
        int band = row / XYTerrainFormat.BandRows;
        byte[] raw = ReadBand(stream, manifest.Blocks[band], band);
        int index = ((row % XYTerrainFormat.BandRows) * NasademTile.Side + column) * 2;
        short value = BinaryPrimitives.ReadInt16LittleEndian(raw.AsSpan(index, 2));
        return value == short.MinValue ? null : value;
    }

    private static byte[] ReadBand(Stream stream, XYTerrainBlock block, int band)
    {
        stream.Position = block.Offset;
        var packed = new byte[block.Length];
        stream.ReadExactly(packed);
        int rows = Math.Min(XYTerrainFormat.BandRows,
            NasademTile.Side - band * XYTerrainFormat.BandRows);
        return XYTerrainCodec.Decode(packed, block.Codec, rows * NasademTile.Side * 2);
    }
}