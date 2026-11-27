using System.Numerics;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ViewportDomain
{
    [Fact]
    public void TaiwanCatalogRootsCoverEveryImportedCellWithoutFillingMissingCells()
    {
        (int, int)[] cells = [(21,120), (21,121), (22,120), (22,121),
            (23,119), (23,120), (23,121), (24,120), (24,121), (24,122), (25,121), (25,122)];
        var domain = new GeoUnifiedViewportDomain(cells);
        Assert.Equal(48, domain.Roots.Count);
        Assert.All(domain.Roots, root => Assert.True(domain.Intersects(root)));
        double area = 0;
        foreach (var root in domain.Roots)
        {
            Vector4[] rects = domain.ClipRects(root);
            Assert.InRange(rects.Length, 1, 4);
            foreach (var rect in rects)
            {
                Assert.InRange(rect.X, 0, 32); Assert.InRange(rect.Y, 0, 32);
                Assert.InRange(rect.Z, rect.X, 32); Assert.InRange(rect.W, rect.Y, 32);
                double step = 1 << root.Level;
                area += (rect.Z - rect.X) * (rect.W - rect.Y) * step * step;
            }
        }
        Assert.Equal(12d * 3600 * 3600, area);
        int span = 32 << GeoUnifiedViewportDomain.RootLevel;
        var outside = new GeoUnifiedTile(GeoUnifiedViewportDomain.RootLevel,
            (118 + 180) * 3600 / span, (21 + 90) * 3600 / span);
        Assert.False(domain.Intersects(outside));
        Assert.Empty(domain.ClipRects(outside));
    }
}