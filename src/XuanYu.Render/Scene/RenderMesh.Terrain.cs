namespace XuanYu.Render;

public sealed partial class RenderMesh
{
    public static RenderMesh FromIndexedTerrain(string assetId,
        ReadOnlySpan<float> vertices, ReadOnlySpan<uint> indices) =>
        new(assetId, vertices, indices);

    public static RenderMesh FromLocalTerrain(string assetId, ReadOnlySpan<float> vertices) =>
        CreateTerrain(assetId, vertices, default, heightOffset: 2);

    public static RenderMesh FromLocalTerrain(string assetId, ReadOnlySpan<float> vertices,
        ReadOnlySpan<float> shaded)
    {
        if (shaded.Length != vertices.Length / 7 * 3)
            throw new ArgumentException("晕渲颜色须与网格顶点一一对应。", nameof(shaded));
        return CreateTerrain(assetId, vertices, shaded.ToArray(), heightOffset: 2);
    }

    // The public RGBA layout remains unchanged for picking and selection.
    public static RenderMesh FromOpaqueTerrain(string assetId, ReadOnlySpan<float> packed) =>
        CreateTerrain(assetId, packed, default);

    public static RenderMesh FromOpaqueTerrain(string assetId, ReadOnlySpan<float> packed,
        ReadOnlySpan<double> geographic)
    {
        if (!geographic.IsEmpty && geographic.Length != packed.Length / 7 * 2)
            throw new ArgumentException("地理坐标通道须与网格顶点一一对应。", nameof(geographic));
        return CreateTerrain(assetId, packed, default,
            geographic.IsEmpty ? default : geographic.ToArray());
    }

    public static RenderMesh FromLitTerrain(string assetId, ReadOnlySpan<float> packed) =>
        FromLitTerrain(assetId, packed, default);

    public static RenderMesh FromLitTerrain(string assetId, ReadOnlySpan<float> packed,
        ReadOnlySpan<double> geographic)
    {
        if (packed.IsEmpty || packed.Length % 30 != 0)
            throw new ArgumentException("晕渲地形须每三顶点一组，每顶点十个 float。", nameof(packed));
        int count = packed.Length / 10;
        if (!geographic.IsEmpty && geographic.Length != count * 2)
            throw new ArgumentException("地理坐标通道须与网格顶点一一对应。", nameof(geographic));
        var vertices = new float[count * 7];
        var shaded = new float[count * 3];
        for (int i = 0; i < count; i++)
        {
            packed.Slice(i * 10, 7).CopyTo(vertices.AsSpan(i * 7, 7));
            packed.Slice(i * 10 + 7, 3).CopyTo(shaded.AsSpan(i * 3, 3));
        }
        return CreateTerrain(assetId, vertices, shaded,
            geographic.IsEmpty ? default : geographic.ToArray());
    }

    private static RenderMesh CreateTerrain(string assetId, ReadOnlySpan<float> packed,
        ReadOnlyMemory<float> shaded, ReadOnlyMemory<double> geographic = default,
        int heightOffset = 6)
    {
        var heights = new float[packed.Length / 7];
        var mesh = new RenderMesh(assetId, packed)
        {
            TerrainElevations = heights, TerrainShadedColors = shaded,
            TerrainGeographicCoordinates = geographic
        };
        for (int i = 0; i < heights.Length; i++)
        {
            heights[i] = mesh._vertices[i * 7 + heightOffset];
            mesh._vertices[i * 7 + 6] = 1f;
        }
        return mesh;
    }
}