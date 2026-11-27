namespace XuanYu.Render;

public static class LocalTerrainVertexChanges
{
    private const int BlockVertices = 96;

    public static bool IsLocal(RenderMesh mesh) =>
        mesh.AssetId.StartsWith("terrain/local/", StringComparison.Ordinal) &&
        !mesh.TerrainElevations.IsEmpty && !mesh.TerrainShadedColors.IsEmpty &&
        mesh.TerrainGeographicCoordinates.IsEmpty;

    public static bool CanReuse(RenderMesh previous, RenderMesh next) =>
        IsLocal(previous) && IsLocal(next) && previous.AssetId == next.AssetId &&
        previous.VertexCount == next.VertexCount;

    public static List<(int First, int Count)> Find(RenderMesh previous, RenderMesh next)
    {
        var ranges = new List<(int First, int Count)>();
        for (int first = 0; first < next.VertexCount; first += BlockVertices)
        {
            int count = Math.Min(BlockVertices, next.VertexCount - first);
            if (SameBlock(previous, next, first, count)) continue;
            if (ranges.Count > 0 && ranges[^1].First + ranges[^1].Count == first)
            {
                var last = ranges[^1];
                ranges[^1] = (last.First, last.Count + count);
            }
            else ranges.Add((first, count));
        }
        return ranges;
    }

    // 比较完整三角形块，包含法线变化产生的晕渲色差异。
    private static bool SameBlock(RenderMesh a, RenderMesh b, int first, int count) =>
        a.Vertices.Span.Slice(first * 7, count * 7).SequenceEqual(
            b.Vertices.Span.Slice(first * 7, count * 7)) &&
        a.TerrainElevations.Span.Slice(first, count).SequenceEqual(
            b.TerrainElevations.Span.Slice(first, count)) &&
        a.TerrainShadedColors.Span.Slice(first * 3, count * 3).SequenceEqual(
            b.TerrainShadedColors.Span.Slice(first * 3, count * 3));
}