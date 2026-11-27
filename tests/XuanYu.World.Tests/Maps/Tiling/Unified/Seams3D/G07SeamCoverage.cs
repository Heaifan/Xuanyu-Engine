using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

internal static class G07SeamCoverage
{
    public static NasademTileCatalog Catalog() => new([
        Environment.GetEnvironmentVariable("XYE_G07_WEST")!,
        Environment.GetEnvironmentVariable("XYE_G07_EAST")!]);

    public static GeoUnifiedTile Root => new(2, 8465, 3184);
    public static GeoCoordinate Origin => new((Root.South + 64) / 3600d - 90,
        (Root.West + 64) / 3600d - 180, 0);

    public static GeoUnifiedCoverage[] Build()
    {
        var root = Root;
        var initial = new GeoUnifiedCoverage(root);
        Assert.True(initial.TryRefine(root, _ => true, out var four));
        var sw = root.Child(0, 0);
        var ne = root.Child(1, 1);
        Assert.True(four.TryRefine(sw, _ => true, out var seven));
        Assert.True(seven.TryRefine(ne, _ => true, out var ten));
        Assert.True(ten.TryCoarsen(ne, _ => true, out var backSeven));
        Assert.True(backSeven.TryCoarsen(sw, _ => true, out var backFour));
        Assert.Equal(2 | 8, ten.EdgeMask(sw.Child(1, 1)));
        Assert.Equal(1 | 4, ten.EdgeMask(ne.Child(0, 0)));
        Assert.Equal(new[] { 4, 7, 10, 7, 4 },
            new[] { four, seven, ten, backSeven, backFour }.Select(c => c.Leaves.Count));
        return [four, seven, ten, backSeven, backFour];
    }
}