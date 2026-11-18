namespace XuanYu.World.Maps;

public static class NasademPyramidPreparation
{
    public static (string Far, string Middle) Prepare(NasademCatalogEntry entry,
        string outputDirectory)
    {
        NasademTile tile = NasademHgtFile.Load(entry.Path);
        if (tile.SouthLatitude != entry.Info.SouthLatitude ||
            tile.WestLongitude != entry.Info.WestLongitude)
            throw new InvalidDataException($"源 HGT 坐标已变更：{entry.Path}");
        Directory.CreateDirectory(outputDirectory);
        string far = Path.Combine(outputDirectory, $"{tile.TileId}.s60.xyheight");
        string middle = Path.Combine(outputDirectory, $"{tile.TileId}.s12.xyheight");
        NasademOverviewFile.Write(far,
            NasademOverviewBuilder.Build(tile, NasademOverviewLevel.Far));
        NasademOverviewFile.Write(middle,
            NasademOverviewBuilder.Build(tile, NasademOverviewLevel.Middle));
        return (far, middle);
    }
}