namespace XuanYu.Render;

public sealed class RenderMesh
{
    private readonly float[] _vertices;

    public string AssetId { get; }
    public ReadOnlyMemory<float> Vertices => _vertices;
    public ReadOnlyMemory<float> TerrainElevations { get; private init; }
    public ReadOnlyMemory<float> TerrainShadedColors { get; private init; }
    public int VertexCount => _vertices.Length / 7;
    public int TriangleCount => VertexCount / 3;

    public RenderMesh(string assetId, ReadOnlySpan<float> vertices)
    {
        if (string.IsNullOrWhiteSpace(assetId))
            throw new ArgumentException(
                "网格资产 ID 不能为空。", nameof(assetId));

        if (vertices.Length == 0 || vertices.Length % 21 != 0)
            throw new ArgumentException(
                "TriangleList 顶点数据必须每三个顶点一组，每个顶点七个 float。",
                nameof(vertices));

        AssetId = assetId;
        _vertices = vertices.ToArray();
    }

    // The public RGBA layout remains unchanged for picking and selection.
    public static RenderMesh FromOpaqueTerrain(string assetId, ReadOnlySpan<float> packed) =>
        CreateTerrain(assetId, packed, default);

    public static RenderMesh FromLitTerrain(string assetId, ReadOnlySpan<float> packed)
    {
        if (packed.IsEmpty || packed.Length % 30 != 0)
            throw new ArgumentException("晕渲地形须每三顶点一组，每顶点十个 float。", nameof(packed));
        int count = packed.Length / 10;
        var vertices = new float[count * 7];
        var shaded = new float[count * 3];
        for (int i = 0; i < count; i++)
        {
            packed.Slice(i * 10, 7).CopyTo(vertices.AsSpan(i * 7, 7));
            packed.Slice(i * 10 + 7, 3).CopyTo(shaded.AsSpan(i * 3, 3));
        }
        return CreateTerrain(assetId, vertices, shaded);
    }

    private static RenderMesh CreateTerrain(string assetId, ReadOnlySpan<float> packed,
        ReadOnlyMemory<float> shaded)
    {
        var heights = new float[packed.Length / 7];
        var mesh = new RenderMesh(assetId, packed)
            { TerrainElevations = heights, TerrainShadedColors = shaded };
        for (int i = 0; i < heights.Length; i++)
        {
            heights[i] = mesh._vertices[i * 7 + 6];
            mesh._vertices[i * 7 + 6] = 1f;
        }
        return mesh;
    }
}