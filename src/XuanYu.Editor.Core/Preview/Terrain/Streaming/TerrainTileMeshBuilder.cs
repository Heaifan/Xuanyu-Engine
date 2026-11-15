using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public static class TerrainTileMeshBuilder
{
    public const int TileCells = 300;

    public static RenderMesh Create(TerrainHeightField source,
        int startX, int startY, int step,
        TerrainWaterMask? water = null,
        (float Min, float Max)? heightRange = null)
    {
        int cellsX = Math.Min(TileCells, source.SamplesX - 1 - startX);
        int cellsY = Math.Min(TileCells, source.SamplesY - 1 - startY);
        if (cellsX % step != 0 || cellsY % step != 0) step = 1;
        var geometry = TerrainTileGeometry.Create(source, startX, startY,
            cellsX, cellsY, step);
        return TerrainRenderMeshAdapter.Create(geometry,
            TerrainDisplayMode.Standard, water, startX: 0, startY: 0,
            spacingX: source.SpacingXMeters, spacingY: source.SpacingYMeters,
            heightRange: heightRange);
    }
}