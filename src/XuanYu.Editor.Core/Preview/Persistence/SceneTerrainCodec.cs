using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Persistence;

internal static class SceneTerrainCodec
{
    public static SceneTerrainData Capture(Guid id, MapDocument map)
    {
        TerrainHeightField field = map.HeightField;
        return new SceneTerrainData(id, field.SamplesX, field.SamplesY,
            field.SpacingXMeters, field.SpacingYMeters, field.Samples.ToArray());
    }

    public static MapDocument RestoreMap(SceneTerrainData data, string name)
    {
        var field = new TerrainHeightField(data.SamplesX, data.SamplesY,
            data.SpacingX, data.SpacingY);
        for (int y = 0; y < field.SamplesY; y++)
            for (int x = 0; x < field.SamplesX; x++)
                field.SetHeight(x, y, data.Heights[y * field.SamplesX + x]);
        return new MapDocument(name, field);
    }

    public static RenderMesh RestoreMesh(string assetId, MapDocument map)
    {
        var geometry = TerrainMeshBuilder.Build(map.HeightField);
        return TerrainRenderMeshAdapter.CreateLocal(assetId, geometry);
    }
}