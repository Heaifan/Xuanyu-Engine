using Xunit;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04NativeSwitch
{
    [Fact]
    public void NearView_UsesOriginalDetailAndFarViewUsesCoarseTiles()
    {
        using var hgt = new G02BHgtFixture("N24E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.Zoom(200);
        IReadOnlyList<XuanYu.Render.RenderDraw> draws = [];
        for (int i = 0; i < 150 && session.NativeDrawCount == 0; i++)
            draws = session.Draws(1000, 700);
        Assert.True(session.NativeDrawCount > 0,
            "近景瓦片必须实际接入 G-04 原始精度网格");
        Assert.Contains(draws, draw =>
            draw.Mesh.AssetId.StartsWith("terrain/g04/", StringComparison.Ordinal));

        session.Camera.Zoom(-200);
        draws = session.Draws(1000, 700);
        Assert.Equal(0, session.NativeDrawCount);
        Assert.All(draws, draw =>
            Assert.StartsWith("terrain/g02/", draw.Mesh.AssetId));
    }
}