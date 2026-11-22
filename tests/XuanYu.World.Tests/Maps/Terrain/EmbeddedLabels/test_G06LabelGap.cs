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
        Assert.True(withoutGap[0].ContourLabelGaps.IsEmpty);
    }

    [Fact]
    public void TwoMajorLinesOnOneDrawKeepTwoMatchingGaps()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        var mesh = RenderMesh.FromOpaqueTerrain("two-lines", new float[] {
            -400,-400,0,1,1,1,0, 400,-400,0,1,1,1,0, 0,400,0,1,1,1,0 });
        var draw = new RenderDraw(mesh, Matrix4x4.Identity, true, false) {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, new short[RenderDemTile.Side * RenderDemTile.Side]),
            DemUV = new float[] { .49f,.49f,.51f,.49f,.5f,.51f } };
        GeoContourLabel Make(int elevation, float east) =>
            new(0, 0, elevation, 0, east, 0, 0, 1, 0, 23, 120) { Owner = mesh };
        var result = session.AttachContourLabelGaps([draw],
            [Make(100, -100), Make(200, 100)], 1280, 720, 1,
            out var visible, _ => 24);
        Assert.Equal(2, visible.Length);
        Assert.Equal(2, result[0].ContourLabelGaps.Length);
        Assert.Equal(new float[] { 100, 200 },
            result[0].ContourLabelGaps.Span.ToArray().Select(g => g.W).ToArray());
    }

    [Fact]
    public void DistantLinesMayShareTheSameElevation()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        var mesh = RenderMesh.FromOpaqueTerrain("same-level", new float[] {
            -400,-400,0,1,1,1,0, 400,-400,0,1,1,1,0, 0,400,0,1,1,1,0 });
        var draw = new RenderDraw(mesh, Matrix4x4.Identity, true, false) {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, new short[RenderDemTile.Side * RenderDemTile.Side]),
            DemUV = new float[] { .49f,.49f,.51f,.49f,.5f,.51f } };
        GeoContourLabel Make(float east, int contourId) =>
            new(0, 0, 100, 0, east, 0, 0, 1, 0, 23, 120)
            { Owner = mesh, ContourId = contourId };
        var result = session.AttachContourLabelGaps([draw],
            [Make(-100, 1), Make(0, 1), Make(100, 2)], 1280, 720, 1,
            out var visible, _ => 24);
        Assert.Equal(new[] { 1, 2 }, visible.Select(x => x.ContourId).ToArray());
        Assert.Equal(2, result[0].ContourLabelGaps.Length);
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