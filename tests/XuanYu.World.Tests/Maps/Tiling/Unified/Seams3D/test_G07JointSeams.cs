using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07JointSeams
{
    private readonly record struct Vertex(short Height, short U, short V, EnuPosition Position);

    [G07DataFact]
    public async Task SharedEdgesAndCornersAgreeBeforeAndAfterMixedLodChanges()
    {
        var catalog = G07SeamCoverage.Catalog();
        var loader = new GeoUnifiedTileLoader(catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(catalog, "joint-seams", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(catalog, loader, cache);
        var anchor = new EnuFrame(G07SeamCoverage.Origin);
        var root = G07SeamCoverage.Root;
        var center = (root.West + 64, root.South + 64);
        const int hgtBoundary = (121 + 180) * 3600;
        foreach (var cover in G07SeamCoverage.Build())
        {
            var vertices = new Dictionary<GeoUnifiedTile, Dictionary<(int X, int Y), Vertex>>();
            foreach (var tile in cover.Leaves)
            {
                using var lease = await pages.RequestAsync(tile);
                vertices.Add(tile, ReadVertices(lease.Page, cover.EdgeMask(tile), anchor));
            }
            Assert.Equal(4, vertices.Values.Count(v => v.ContainsKey(center)));
            bool crossesHgt = false;
            var tiles = cover.Leaves.ToArray();
            for (int i = 0; i < tiles.Length; i++)
                for (int j = i + 1; j < tiles.Length; j++)
                    crossesHgt |= CheckShared(tiles[i], tiles[j], vertices, hgtBoundary);
            Assert.True(crossesHgt, "必须检查经度121度处的公共边顶点。");
        }
    }

    private static Dictionary<(int X, int Y), Vertex> ReadVertices(GeoUnifiedHeightPage page,
        int mask, EnuFrame anchor)
    {
        var result = new Dictionary<(int, int), Vertex>();
        var tile = page.Tile;
        int step = 1 << tile.Level;
        foreach (int i in GeoUnifiedGridIndices.Create(mask).Distinct())
        {
            int col = i % 33, row = i / 33;
            int x = tile.West + col * step, y = tile.South + row * step;
            short h = page.HeightAt(col, row)!.Value;
            result.Add((x, y), new(h, page.EncodedNormals.Span[i],
                page.EncodedNormals.Span[1089 + i], GeoUnifiedPosition.At(anchor, x, y, h)));
        }
        return result;
    }

    private static bool CheckShared(GeoUnifiedTile a, GeoUnifiedTile b,
        Dictionary<GeoUnifiedTile, Dictionary<(int X, int Y), Vertex>> vertices, int boundary)
    {
        int x0 = Math.Max(a.West, b.West), x1 = Math.Min(a.West + a.Span, b.West + b.Span);
        int y0 = Math.Max(a.South, b.South), y1 = Math.Min(a.South + a.Span, b.South + b.Span);
        if (x0 > x1 || y0 > y1) return false;
        Assert.True(x0 == x1 || y0 == y1, "已提交覆盖不允许面积重叠。");
        var left = vertices[a]; var right = vertices[b];
        var keys = left.Keys.Where(InShared).OrderBy(k => k.X).ThenBy(k => k.Y).ToArray();
        Assert.NotEmpty(keys);
        Assert.Equal(keys, right.Keys.Where(InShared).OrderBy(k => k.X).ThenBy(k => k.Y).ToArray());
        foreach (var key in keys) Assert.Equal(left[key], right[key]);
        return keys.Any(k => k.X == boundary);

        bool InShared((int X, int Y) k) => k.X >= x0 && k.X <= x1 && k.Y >= y0 && k.Y <= y1;
    }
}