using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

internal static class GeoContourLabelWater
{
    private const int Cells = 3600;
    private const int Side = Cells + 1;
    private const int Words = (Side * Side + 15) / 16;

    public static bool NearWater(RenderWaterAtlas? atlas, GeoCoordinate geo)
    {
        if (atlas is null) return false;
        ReadOnlySpan<short> data = atlas.PackedSamples.Span;
        if (data.Length < 3 || data[0] <= 0 || data[1] <= 0) return false;
        int columns = data[0], rows = data[1];
        double x = (geo.LongitudeDegrees - atlas.WestLongitude) * Cells;
        double y = (geo.LatitudeDegrees - atlas.SouthLatitude) * Cells;
        int cx = (int)Math.Floor(x), cy = (int)Math.Floor(y);
        for (int dy = -2; dy <= 3; dy++)
            for (int dx = -2; dx <= 3; dx++)
            {
                int px = cx + dx, py = cy + dy;
                int tx = (int)Math.Floor(px / (double)Cells);
                int ty = (int)Math.Floor(py / (double)Cells);
                if (tx < 0 || tx >= columns || ty < 0 || ty >= rows) continue;
                int headerIndex = 2 + ty * columns + tx;
                if (headerIndex >= data.Length) return true;
                int slot = data[headerIndex];
                if (slot <= 0) continue;
                int sample = (py - ty * Cells) * Side + px - tx * Cells;
                int word = 2 + columns * rows + (slot - 1) * Words + sample / 16;
                if (word >= data.Length || ((ushort)data[word] & (1 << (sample % 16))) != 0)
                    return true;
            }
        return false;
    }
}
