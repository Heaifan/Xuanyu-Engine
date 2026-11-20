
namespace XuanYu.Render;

public sealed class RenderMesh
{
    private readonly float[] _vertices;

    public string AssetId { get; }
    public ReadOnlyMemory<float> Vertices => _vertices;
    public ReadOnlyMemory<float> TerrainElevations { get; private init; }
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

    // Terrain builders submit opaque RGB vertices with the sampled elevation in slot seven.
    // Keep the public RGBA layout intact for picking, selection and non-Vulkan consumers.
    public static RenderMesh FromOpaqueTerrain(string assetId, ReadOnlySpan<float> packed)
    {
        var heights = new float[packed.Length / 7];
        var mesh = new RenderMesh(assetId, packed) { TerrainElevations = heights };
        for (int i = 0; i < heights.Length; i++)
        {
            heights[i] = mesh._vertices[i * 7 + 6];
            mesh._vertices[i * 7 + 6] = 1f;
        }
        return mesh;
    }
}
