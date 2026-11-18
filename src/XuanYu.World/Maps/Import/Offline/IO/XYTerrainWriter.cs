using System.Buffers.Binary;

namespace XuanYu.World.Maps;

public readonly record struct XYTerrainWriteResult(long RawBytes, long PackBytes,
    int RawBlocks, int DeflateBlocks, int BrotliBlocks, int DeltaBrotliBlocks)
{
    public double Ratio => (double)PackBytes / RawBytes;
}

public static class XYTerrainWriter
{
    public static XYTerrainWriteResult Write(string sourcePath, string destinationPath) =>
        Write(NasademHgtFile.Load(sourcePath), destinationPath);

    public static XYTerrainWriteResult Write(NasademTile tile, string destinationPath)
    {
        ArgumentNullException.ThrowIfNull(tile);
        string path = Path.GetFullPath(destinationPath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = File.Create(path);
        using var writer = new BinaryWriter(stream);
        var blocks = new XYTerrainBlock[XYTerrainFormat.BlockCount];
        XYTerrainFormat.Write(writer, tile, blocks);
        int[] counts = new int[4];
        for (int band = 0; band < blocks.Length; band++)
        {
            int row = band * XYTerrainFormat.BandRows;
            int rows = Math.Min(XYTerrainFormat.BandRows, NasademTile.Side - row);
            var raw = new byte[rows * NasademTile.Side * sizeof(short)];
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < NasademTile.Side; x++)
                {
                    short value = tile.HeightAt(x, NasademTile.Side - 1 - row - y)
                        ?? short.MinValue;
                    BinaryPrimitives.WriteInt16LittleEndian(
                        raw.AsSpan((y * NasademTile.Side + x) * 2, 2), value);
                }
            var (payload, method) = XYTerrainCodec.Encode(raw);
            blocks[band] = new XYTerrainBlock(stream.Position, payload.Length, method);
            writer.Write(payload);
            counts[method]++;
        }
        long bytes = stream.Length;
        stream.Position = 0;
        XYTerrainFormat.Write(writer, tile, blocks);
        return new XYTerrainWriteResult(NasademTile.ByteLength, bytes,
            counts[0], counts[1], counts[2], counts[3]);
    }
}