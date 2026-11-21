using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed record TerrainDemEntry(NasademHgtInfo Info, string Path, bool HasWaterMask)
{
    public string TileId => Info.TileId;
    public GeoTileBounds Bounds => new(Info.SouthLatitude, Info.WestLongitude,
        Info.SouthLatitude + 1d, Info.WestLongitude + 1d);

    internal static TerrainDemEntry Create(NasademCatalogEntry source) =>
        new(source.Info, source.Path, NasademWaterFile.HasMask(source.Path, source.Info));
}