using System.IO.Compression;

namespace XuanYu.World.Maps;

internal static class XYTerrainCodec
{
    // 0=raw, 1=Deflate, 2=Brotli, 3=逐行 Delta16+Brotli。
    public static (byte[] Payload, int Method) Encode(byte[] raw)
    {
        byte[] best = raw;
        int method = 0;
        for (int candidate = 1; candidate <= 3; candidate++)
        {
            byte[] input = candidate == 3 ? XYTerrainDelta.Forward(raw) : raw;
            byte[] packed = Compress(input, candidate);
            if (packed.Length >= best.Length) continue;
            best = packed;
            method = candidate;
        }
        return (best, method);
    }

    public static byte[] Decode(byte[] payload, int method, int rawBytes)
    {
        if (method == 0)
        {
            if (payload.Length != rawBytes)
                throw new InvalidDataException("XYTerrain 原始块长度不正确。");
            return payload;
        }
        using var input = new MemoryStream(payload, false);
        using Stream decoder = method == 1
            ? new DeflateStream(input, CompressionMode.Decompress)
            : new BrotliStream(input, CompressionMode.Decompress);
        var raw = new byte[rawBytes];
        decoder.ReadExactly(raw);
        if (decoder.ReadByte() != -1)
            throw new InvalidDataException("XYTerrain 解压数据超出块长度。");
        if (method == 3) XYTerrainDelta.Reverse(raw);
        return raw;
    }

    private static byte[] Compress(byte[] raw, int method)
    {
        using var output = new MemoryStream();
        using (Stream encoder = method == 1
            ? new DeflateStream(output, CompressionLevel.Optimal, true)
            : new BrotliStream(output, CompressionLevel.Optimal, true))
            encoder.Write(raw);
        return output.ToArray();
    }
}