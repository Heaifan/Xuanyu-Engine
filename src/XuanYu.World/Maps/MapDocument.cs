namespace XuanYu.World.Maps;

public sealed class MapDocument
{
    public const int CurrentVersion = 1;
    public int Version => CurrentVersion;
    public string Name { get; }
    public TerrainHeightField HeightField { get; }
    public TerrainWaterMask? WaterMask { get; }
    public MapGeoReference? GeoReference { get; }
    public float WidthMeters => HeightField.WidthMeters;
    public float LengthMeters => HeightField.LengthMeters;

    public MapDocument(string name, TerrainHeightField heightField,
        TerrainWaterMask? waterMask = null,
        MapGeoReference? geoReference = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("地图名称不能为空。", nameof(name));
        ArgumentNullException.ThrowIfNull(heightField);
        if (waterMask is not null &&
            (waterMask.SamplesX != heightField.SamplesX ||
             waterMask.SamplesY != heightField.SamplesY))
            throw new ArgumentException("水体掩膜与高程采样尺寸不一致。", nameof(waterMask));
        Name = name.Trim();
        HeightField = heightField;
        WaterMask = waterMask;
        GeoReference = geoReference;
    }

    public static MapDocument CreateDefault(string name) =>
        new(name, new TerrainHeightField(129, 129, 2f));
}