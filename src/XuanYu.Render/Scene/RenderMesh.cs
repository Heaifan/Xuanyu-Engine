namespace XuanYu.Render;

public sealed class RenderMesh
{
    private readonly float[] _vertices;
    private readonly uint[] _indices;

    public string AssetId { get; }
    public ReadOnlyMemory<float> Vertices => _vertices;
    public ReadOnlyMemory<float> TerrainElevations { get; private init; }
    public ReadOnlyMemory<float> TerrainShadedColors { get; private init; }
    // 每顶点 longitude、latitude（度）；NaN 表示沿用几何反算。
    public ReadOnlyMemory<double> TerrainGeographicCoordinates { get; private init; }
    public int VertexCount => _vertices.Length / 7;
    public int IndexCount => _indices.Length;
    public ReadOnlyMemory<uint> Indices => _indices;
    public int TriangleCount => (_indices.Length == 0 ? VertexCount : _indices.Length) / 3;

    public RenderMesh(string assetId, ReadOnlySpan<float> vertices,
        ReadOnlySpan<uint> indices = default)
    {
        if (string.IsNullOrWhiteSpace(assetId))
            throw new ArgumentException(
                "网格资产 ID 不能为空。", nameof(assetId));

        if (vertices.Length == 0 || vertices.Length % 7 != 0 ||
            (indices.IsEmpty && vertices.Length % 21 != 0) || indices.Length % 3 != 0)
            throw new ArgumentException(
                "三角网格必须使用七 float 顶点，非索引网格每三个顶点一组。",
                nameof(vertices));

        AssetId = assetId;
        _vertices = vertices.ToArray();
        _indices = indices.ToArray();
        if (_indices.Any(index => index >= VertexCount))
            throw new ArgumentOutOfRangeException(nameof(indices), "索引超出顶点范围。");
    }

    public static RenderMesh FromIndexedTerrain(string assetId,
        ReadOnlySpan<float> vertices, ReadOnlySpan<uint> indices) =>
        new(assetId, vertices, indices);
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
        ReadOnlyMemory<float> shaded, ReadOnlyMemory<double> geographic = default)
    {
        var heights = new float[packed.Length / 7];
        var mesh = new RenderMesh(assetId, packed)
        {
            TerrainElevations = heights, TerrainShadedColors = shaded,
            TerrainGeographicCoordinates = geographic
        };
        for (int i = 0; i < heights.Length; i++)
        {
            heights[i] = mesh._vertices[i * 7 + 6];
            mesh._vertices[i * 7 + 6] = 1f;
        }
        return mesh;
    }
}