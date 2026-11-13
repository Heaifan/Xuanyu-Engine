
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class TerrainPreviewMap
{
    public static MapDocument Create()
    {
        MapDocument map = MapDocument.CreateDefault("E4 地形预览");
        TerrainHeightField field = map.HeightField;

        for (int y = 0; y < field.SamplesY; y++)
        {
            for (int x = 0; x < field.SamplesX; x++)
            {
                float px = x * field.SpacingMeters;
                float py = y * field.SpacingMeters;

                float ridge = 12f * MathF.Sin(px * 0.035f)
                    * MathF.Cos(py * 0.045f);

                float dx = px - 155f;
                float dy = py - 105f;
                float peak = 38f * MathF.Exp(
                    -(dx * dx + dy * dy) / 9000f);

                dx = px - 65f;
                dy = py - 175f;
                float valley = 15f * MathF.Exp(
                    -(dx * dx + dy * dy) / 4500f);

                field.SetHeight(x, y, ridge + peak - valley);
            }
        }

        return map;
    }
}
