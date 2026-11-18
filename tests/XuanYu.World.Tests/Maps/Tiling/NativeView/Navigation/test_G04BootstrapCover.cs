using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G04BootstrapCover
{
    [Fact]
    public void FirstIncompleteFrame_UsesFullLowResolutionCover()
    {
        using var hgt = new G02BHgtFixture("N23E121");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(
            new GeoCoordinate(23.5d, 121.5d, 0d), 8000d);

        var draws = session.Draws(1280, 720);

        Assert.True(session.SelectedCount > 4);
        Assert.True(session.BootstrapActive);
        Assert.Single(draws);
        Assert.Equal(16 * 16 * 6, draws[0].Mesh.VertexCount);
    }
}