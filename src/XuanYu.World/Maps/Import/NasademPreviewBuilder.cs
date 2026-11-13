
namespace XuanYu.World.Maps;

public static class NasademPreviewBuilder
{
    private const int PreviewRows = 129;
    private const int SourceSteps = NasademTile.Side - 1;

    public static MapDocument Create(NasademTile tile)
    {
        var geo = new NasademGeoReference(tile);
        float spacing = (float)(geo.LengthMeters / (PreviewRows - 1));
        int columns = Math.Max(2,
            (int)Math.Round(geo.WidthMeters / spacing) + 1);

        var field = new TerrainHeightField(columns, PreviewRows, spacing);
        for (int y = 0; y < PreviewRows; y++)
        {
            double sourceY = (double)y * SourceSteps / (PreviewRows - 1);
            for (int x = 0; x < columns; x++)
            {
                double sourceX = (double)x * SourceSteps / (columns - 1);
                field.SetHeight(x, y, Sample(tile, sourceX, sourceY));
            }
        }

        return new MapDocument($"NASADEM_{tile.TileId}", field);
    }

    private static float Sample(NasademTile tile, double x, double y)
    {
        int west = (int)Math.Floor(x);
        int east = Math.Min(west + 1, SourceSteps);
        int south = (int)Math.Floor(y);
        int north = Math.Min(south + 1, SourceSteps);
        double fx = x - west;
        double fy = y - south;
        double sum = 0;
        double total = 0;

        Accumulate(west, south, (1 - fx) * (1 - fy));
        Accumulate(east, south, fx * (1 - fy));
        Accumulate(west, north, (1 - fx) * fy);
        Accumulate(east, north, fx * fy);

        // 无有效高程时仅在预览中以海平面补值，原始 HGT 不变。
        return total > 0 ? (float)(sum / total) : 0f;

        void Accumulate(int column, int row, double weight)
        {
            if (weight <= 0)
                return;

            if (tile.HeightAt(column, row) is not { } height)
                return;

            sum += height * weight;
            total += weight;
        }
    }
}
