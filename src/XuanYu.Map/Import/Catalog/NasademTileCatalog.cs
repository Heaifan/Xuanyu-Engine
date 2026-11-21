using System.Linq;

namespace XuanYu.World.Maps;

public sealed class NasademTileCatalog
{
    private readonly Dictionary<(int South, int West), NasademCatalogEntry> _entries = [];

    public int Count => _entries.Count;
    public IReadOnlyCollection<(int South, int West)> Coordinates => _entries.Keys;
    public IReadOnlyCollection<NasademCatalogEntry> Entries => _entries.Values;

    public NasademTileCatalog(IEnumerable<string> paths)
    {
        ArgumentNullException.ThrowIfNull(paths);
        foreach (string path in paths)
        {
            string fullPath = Path.GetFullPath(path);
            NasademHgtInfo info = NasademHgtFile.ReadInfo(fullPath);
            if (!_entries.TryAdd((info.SouthLatitude, info.WestLongitude), new(info, fullPath)))
                throw new InvalidDataException($"重复的 NASADEM 瓦片：{info.TileId}");
        }
    }

    public static NasademTileCatalog Scan(string directory) =>
        new(Directory.EnumerateFiles(directory).Where(IsSupported));

    public NasademTile? Load(int south, int west)
    {
        if (!_entries.TryGetValue((south, west), out var entry)) return null;
        NasademTile tile = NasademHgtFile.Load(entry.Path);
        if (tile.SouthLatitude != south || tile.WestLongitude != west)
            throw new InvalidDataException($"HGT 文件已变更：{entry.Path}");
        return tile;
    }

    public TerrainWaterMask? LoadWaterMask(int south, int west) =>
        _entries.TryGetValue((south, west), out var entry)
            ? NasademWaterFile.Load(entry.Path, entry.Info) : null;

    private static bool IsSupported(string path)
    {
        string extension = Path.GetExtension(path);
        return extension.Equals(".hgt", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".zip", StringComparison.OrdinalIgnoreCase) ||
            extension.Equals(".xyt", StringComparison.OrdinalIgnoreCase);
    }
}