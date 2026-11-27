namespace XuanYu.Editor.Preview.Terrain;

public sealed record GeoTerrainMiniMapData(
    double South, double West, double North, double East,
    int Columns, int Rows, byte[] Cells)
{
    // 本地数据使用米制 X/Y 范围，不能应用纬度余弦修正。
    public bool IsLocal { get; init; }

    internal static byte HeightBand(double height) => height switch
    {
        < 100d => 2, < 250d => 3, < 450d => 4,
        < 650d => 5, < 900d => 6, < 1300d => 7,
        < 1800d => 8, < 2400d => 9, < 3000d => 10,
        < 3600d => 11, < 4200d => 12, _ => 13
    };
}