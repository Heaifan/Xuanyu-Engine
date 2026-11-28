namespace XuanYu.Render;

public sealed partial class RenderMesh
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
}