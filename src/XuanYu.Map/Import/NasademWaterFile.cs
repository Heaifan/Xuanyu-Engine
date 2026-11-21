using System.IO.Compression;

namespace XuanYu.World.Maps;

public static class NasademWaterFile
{
    public static bool HasMask(string path, NasademHgtInfo info)
    {
        string extension = Path.GetExtension(path);
        if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ZipFile.OpenRead(path);
            return FindEntry(archive, info.TileId) is not null;
        }
        return extension.Equals(".hgt", StringComparison.OrdinalIgnoreCase) &&
            File.Exists(Path.ChangeExtension(path, ".swb"));
    }

    public static TerrainWaterMask? Load(string path, NasademTile tile) =>
        Load(path, new NasademHgtInfo(tile.TileId, tile.SouthLatitude, tile.WestLongitude));

    public static TerrainWaterMask? Load(string path, NasademHgtInfo info)
    {
        string extension = Path.GetExtension(path);
        if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var archive = ZipFile.OpenRead(path);
            var entry = FindEntry(archive, info.TileId);
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

    private static ZipArchiveEntry? FindEntry(ZipArchive archive, string tileId) =>
        archive.Entries.SingleOrDefault(item =>
            Path.GetExtension(item.Name).Equals(".swb", StringComparison.OrdinalIgnoreCase) &&
            Path.GetFileNameWithoutExtension(item.Name).EndsWith(
                tileId, StringComparison.OrdinalIgnoreCase));

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