using XuanYu.World.Maps;
using XuanYu.Render;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07GridTopology
{
    [Theory]
    [InlineData(0)] [InlineData(5)] [InlineData(10)] [InlineData(15)]
    public void SharedTopologyCanBeUploadedAsIndexedTerrainMesh(int mask)
    {
        float[] vertices = new float[33 * 33 * 7];
        uint[] indices = GeoUnifiedGridIndices.Create(mask).Select(i => (uint)i).ToArray();
        var mesh = RenderMesh.FromIndexedTerrain("g07-test", vertices, indices);
        Assert.Equal(33 * 33, mesh.VertexCount);
        Assert.Equal(indices.Length, mesh.IndexCount);
        Assert.Equal(indices.Length / 3, mesh.TriangleCount);
        Assert.Equal(indices, mesh.Indices.ToArray());
    }

    [Theory]
    [InlineData(0)] [InlineData(1)] [InlineData(2)] [InlineData(3)]
    [InlineData(4)] [InlineData(5)] [InlineData(6)] [InlineData(7)]
    [InlineData(8)] [InlineData(9)] [InlineData(10)] [InlineData(11)]
    [InlineData(12)] [InlineData(13)] [InlineData(14)] [InlineData(15)]
    public void Mask_PreservesAreaWindingAndBoundary(int mask)
    {
        int[] indices = GeoUnifiedGridIndices.Create(mask);
        double area = 0;
        var edges = new Dictionary<(int, int), int>();
        for (int i = 0; i < indices.Length; i += 3)
        {
            int a = indices[i], b = indices[i + 1], c = indices[i + 2];
            int ax = a % 33, ay = a / 33, bx = b % 33, by = b / 33;
            int cx = c % 33, cy = c / 33;
            int cross = (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
            Assert.True(cross > 0);
            area += cross / 2d;
            Add(a, b); Add(b, c); Add(c, a);
        }
        Assert.Equal(1024, area);
        foreach (var (edge, count) in edges)
        {
            Assert.InRange(count, 1, 2);
            if (count == 2) continue;
            int ax = edge.Item1 % 33, ay = edge.Item1 / 33;
            int bx = edge.Item2 % 33, by = edge.Item2 / 33;
            bool boundary = ay == 0 && by == 0 || ay == 32 && by == 32 ||
                ax == 0 && bx == 0 || ax == 32 && bx == 32;
            Assert.True(boundary);
            int bit = ay == 0 && by == 0 ? 1 : ay == 32 && by == 32 ? 2 :
                ax == 0 && bx == 0 ? 4 : 8;
            Assert.Equal((mask & bit) == 0 ? 1 : 2,
                Math.Abs(ax - bx) + Math.Abs(ay - by));
        }
        void Add(int x, int y)
        {
            var key = x < y ? (x, y) : (y, x);
            edges[key] = edges.GetValueOrDefault(key) + 1;
        }
    }
}
