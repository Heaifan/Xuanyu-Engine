using System.Numerics;
using Xunit;
using Xunit.Abstractions;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

internal static class G06GeometryCompare
{
    public static void Run(ITestOutputHelper output, NasademTileCatalog catalog,
        NasademTile raw, EnuFrame anchor, GeoTileId id, RenderDraw actual,
        IReadOnlyList<Vector3> samples)
    {
        var source = new NasademElevationSource(catalog, 16, useOverview: true);
        var coverage = new GeoTileBounds(raw.SouthLatitude, raw.WestLongitude,
            raw.SouthLatitude + 1d, raw.WestLongitude + 1d);
        RenderMesh middle = Build(source.CoarseFor(id.Level), id, anchor, coverage);
        RenderMesh original = Build(new NasademElevationSource(raw), id, anchor, coverage);
        Assert.Equal(1536, actual.Mesh.VertexCount);
        Assert.Equal(actual.Mesh.VertexCount, middle.VertexCount);
        Assert.Equal(middle.VertexCount, original.VertexCount);
        AssertSameTopology(middle, original);
        double controlError = 0d;
        foreach (Vector3 p in samples)
        {
            double? a = GeoTerrainMeshSurface.HeightAt(actual.Mesh, p.X, p.Y);
            double? b = GeoTerrainMeshSurface.HeightAt(middle, p.X, p.Y);
            double? c = GeoTerrainMeshSurface.HeightAt(original, p.X, p.Y);
            Assert.True(a.HasValue && b.HasValue && c.HasValue, "对照网格存在未命中采样。");
            controlError = Math.Max(controlError, Math.Abs(a!.Value - b!.Value));
        }
        output.WriteLine(FormattableString.Invariant(
            $"[G06/R04B] ID={id} 样本={samples.Count} 顶点={middle.VertexCount} XY拓扑一致=True 实际与对照最大差(m)={controlError:F6}"));
        foreach (var (label, mesh) in new[] { ("Middle12-Control", middle), ("Raw1-Nodes16", original) })
        {
            var draw = new RenderDraw(mesh, actual.ModelToClip, true, false);
            output.WriteLine(G06GeometryMeasure.Report(label, draw, actual.ModelToClip, samples)
                .Replace("[G06/R04A]", "[G06/R04B]", StringComparison.Ordinal));
        }
        Assert.InRange(controlError, 0d, 0.1d);
    }

    private static RenderMesh Build(NasademElevationSource source, GeoTileId id,
        EnuFrame anchor, GeoTileBounds coverage) =>
        GeoTerrainMeshBuilder.Build(GeoHeightTileBuilder.Build(source, id, 16),
            anchor, source, coverage) ?? throw new InvalidOperationException("对照网格未生成。");

    private static void AssertSameTopology(RenderMesh a, RenderMesh b)
    {
        var left = a.Vertices.Span;
        var right = b.Vertices.Span;
        // TriangleList 顶点顺序相同才算相同三角形拓扑，不只比较总数。
        for (int i = 0; i < left.Length; i += 7)
        {
            Assert.Equal(left[i], right[i]);
            Assert.Equal(left[i + 1], right[i + 1]);
        }
    }
}