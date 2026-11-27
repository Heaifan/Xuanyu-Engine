using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07GeographicNormals
{
    [Fact]
    public async Task SharedPositionsHaveIdenticalEncodedNormalsAcrossPagesAndLevels()
    {
        using var fixture = new G07NormalFixture();
        var loader = new GeoUnifiedTileLoader(fixture.Catalog, 256L * 1024 * 1024);
        var source = new GeoUnifiedSamples(fixture.Catalog, "normal-test", loader);
        var cache = new GeoUnifiedPageCache(loader, source);
        var pages = new GeoUnifiedPageLoader(fixture.Catalog, loader, cache);
        using var left = await pages.RequestAsync(G07NormalFixture.Fine);
        using var right = await pages.RequestAsync(G07NormalFixture.Right);
        using var coarse = await pages.RequestAsync(G07NormalFixture.Coarse);
        for (int row = 0; row <= 32; row++)
        {
            Assert.Equal(left.Page.HeightAt(32, row), right.Page.HeightAt(0, row));
            Assert.Equal(Normal(left.Page, 32, row), Normal(right.Page, 0, row));
            if (row % 2 == 0)
                Assert.Equal(Normal(left.Page, 32, row), Normal(coarse.Page, 0, row / 2));
        }
        // x=121°，一侧数据来自西HGT，另一侧来自东HGT。
        Assert.Equal(Normal(right.Page, 16, 16), Normal(coarse.Page, 8, 8));
        Assert.InRange(Normal(right.Page, 16, 16).U, (short)23000, (short)27000);
        Assert.Equal((short)0, Normal(right.Page, 16, 16).V);
        // 局部非线性样本确实改变坡度，不能用恒定法线使测试通过。
        Assert.NotEqual(Normal(right.Page, 0, 16), Normal(right.Page, 0, 14));
        Assert.Equal(2178, left.Page.EncodedNormals.Length);
    }

    private static (short U, short V) Normal(GeoUnifiedHeightPage page, int x, int y)
    {
        int i = y * 33 + x;
        return (page.EncodedNormals.Span[i], page.EncodedNormals.Span[1089 + i]);
    }
}