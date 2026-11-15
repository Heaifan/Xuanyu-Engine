using System.IO.Compression;

namespace XuanYu.World.Maps;

public static class NasademWaterFile
{
    public static TerrainWaterMask? Load(string path, NasademTile tile)
    {
        string extension = Path.GetExtension(path);
        if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ZipFile.OpenRead(path);
            var entry = archive.Entries.SingleOrDefault(item =>
                Path.GetExtension(item.Name).Equals(".swb",
                    StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileNameWithoutExtension(item.Name).EndsWith(
                    tile.TileId, StringComparison.OrdinalIgnoreCase));
            if (entry is null) return null;
            using var input = entry.Open();
            return Read(input, entry.Length);
        }
        if (extension.Equals(".hgt", StringComparison.OrdinalIgnoreCase))
        {
            string sidecar = Path.ChangeExtension(path, ".swb");
            if (!File.Exists(sidecar)) return null;
            using var input = File.OpenRead(sidecar);
            return Read(input, input.Length);
        }
        return null;
    }

    private static TerrainWaterMask Read(Stream source, long length)
    {
        if (length != NasademTile.SampleCount)
            throw new InvalidDataException("SWB 尺寸必须为 3601×3601 字节。");
        byte[] raw = new byte[NasademTile.SampleCount];
        source.ReadExactly(raw);
        byte[] southFirst = new byte[raw.Length];
        int side = NasademTile.Side;
        for (int y = 0; y < side; y++)
            Array.Copy(raw, (side - 1 - y) * side, southFirst, y * side, side);
        return new TerrainWaterMask(side, side, southFirst);
    }
}