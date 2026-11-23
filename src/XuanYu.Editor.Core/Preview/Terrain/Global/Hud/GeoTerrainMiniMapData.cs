namespace XuanYu.Editor.Preview.Terrain;

public sealed record GeoTerrainMiniMapData(
    double South, double West, double North, double East,
    int Columns, int Rows, byte[] Cells);