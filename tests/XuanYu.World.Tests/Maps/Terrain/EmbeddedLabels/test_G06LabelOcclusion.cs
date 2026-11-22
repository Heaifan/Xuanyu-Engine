using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06LabelOcclusion
{
    [Fact]
    public void ForegroundTerrainHidesLabelAndItsGap()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        RenderMesh Make(float z) => RenderMesh.FromOpaqueTerrain($"mesh-{z}", new float[] {
            -200,-200,z,1,1,1,0, 200,-200,z,1,1,1,0, 0,200,z,1,1,1,0 });
        var behind = Make(0);
        var front = Make(100);
        RenderDraw MakeDraw(RenderMesh mesh) => new(mesh, Matrix4x4.Identity, true, false) {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, new short[RenderDemTile.Side * RenderDemTile.Side]),
            DemUV = new float[] { .49f,.49f,.51f,.49f,.5f,.51f } };
        var label = new GeoContourLabel(0, 0, 100, 0, 0, 0, 0, 1, 0, 23, 120) { Owner = behind };
        session.AttachContourLabelGaps([MakeDraw(behind)], [label], 1280, 720, 1,
            out var visible, _ => 20);
        Assert.Single(visible);
        var result = session.AttachContourLabelGaps([MakeDraw(behind), MakeDraw(front)],
            [label], 1280, 720, 1, out visible, _ => 20);
        Assert.Empty(visible);
        Assert.Equal(Vector4.Zero, result[0].ContourLabelGap);
        session.AttachContourLabelGaps([MakeDraw(behind)], [label], 1280, 720, 1,
            out visible, _ => 20);
        Assert.Single(visible);
    }
}
