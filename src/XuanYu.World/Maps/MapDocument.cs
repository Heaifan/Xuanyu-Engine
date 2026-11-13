
namespace XuanYu.World.Maps;

public sealed class MapDocument
{
    public const int CurrentVersion = 1;

    public int Version => CurrentVersion;
    public string Name { get; }
    public TerrainHeightField HeightField { get; }

    public float WidthMeters => HeightField.WidthMeters;
    public float LengthMeters => HeightField.LengthMeters;

    public MapDocument(string name, TerrainHeightField heightField)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "地图名称不能为空。", nameof(name));

        ArgumentNullException.ThrowIfNull(heightField);

        Name = name.Trim();
        HeightField = heightField;
    }

    public static MapDocument CreateDefault(string name)
    {
        return new MapDocument(
            name,
            new TerrainHeightField(129, 129, 2f));
    }
}
