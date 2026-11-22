using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06LabelGap
{
    [Fact]
    public void GapCarriesElevationAndDisappearsWithLabels()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        var mesh = RenderMesh.FromOpaqueTerrain("gap", new float[] {
            -400,-400,0,1,1,1,0, 400,-400,0,1,1,1,0, 0,400,0,1,1,1,0 });
        var draw = new RenderDraw(mesh, Matrix4x4.Identity, true, false) {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, new short[RenderDemTile.Side * RenderDemTile.Side]),
            DemUV = new float[] { .49f,.49f,.51f,.49f,.5f,.51f } };
        var label = new GeoContourLabel(0, 0, 100, 0, 0, 0, 0, 1, 0, 23, 120) { Owner = mesh };
        var withGap = session.AttachContourLabelGaps([draw], [label], 1280, 720, 1, out var visible);
        Assert.Single(visible);
        Assert.Equal(100, withGap[0].ContourLabelGap.W);
        session.SetContourLabelsEnabled(false);
        var withoutGap = session.AttachContourLabelGaps([draw], [label], 1280, 720, 1, out visible);
        Assert.Empty(visible);
        Assert.Equal(Vector4.Zero, withoutGap[0].ContourLabelGap);
    }
    [Fact]
    public void RejectsLabelWhenGapExtendsBeyondOwnerMesh()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        var mesh = RenderMesh.FromOpaqueTerrain("small", new float[] {
            -15,-20,0,1,1,1,0, 15,-20,0,1,1,1,0, 0,20,0,1,1,1,0 });
        var draw = new RenderDraw(mesh, Matrix4x4.Identity, true, false) {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, new short[RenderDemTile.Side * RenderDemTile.Side]),
            DemUV = new float[] { .49f,.49f,.51f,.49f,.5f,.51f } };
        var label = new GeoContourLabel(0, 0, 100, 0, 0, 0, 0, 1, 0, 23, 120) { Owner = mesh };
        var result = session.AttachContourLabelGaps([draw], [label], 1280, 720, 1,
            out var visible, _ => 80);
        Assert.Empty(visible);
        Assert.Equal(Vector4.Zero, result[0].ContourLabelGap);
    }
}
