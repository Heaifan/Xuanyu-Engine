using System.IO.Compression;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed record TerrainDemEntry(NasademHgtInfo Info, string Path, bool HasWaterMask)
{
    public string TileId => Info.TileId;
    public GeoTileBounds Bounds => new(Info.SouthLatitude, Info.WestLongitude,
        Info.SouthLatitude + 1d, Info.WestLongitude + 1d);

    internal static TerrainDemEntry Create(NasademCatalogEntry source)
    {
        string path = source.Path;
        NasademHgtInfo info = source.Info;
        if (!System.IO.Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            return new(info, path, File.Exists(System.IO.Path.ChangeExtension(path, ".swb")));
        using var archive = ZipFile.OpenRead(path);
        bool found = archive.Entries.Any(entry =>
            System.IO.Path.GetExtension(entry.Name).Equals(".swb", StringComparison.OrdinalIgnoreCase) &&
            System.IO.Path.GetFileNameWithoutExtension(entry.Name).EndsWith(
                info.TileId, StringComparison.OrdinalIgnoreCase));
        return new(info, path, found);
    }
}