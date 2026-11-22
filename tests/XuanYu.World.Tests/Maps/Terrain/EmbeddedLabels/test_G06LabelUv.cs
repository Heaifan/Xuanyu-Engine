using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06LabelUv
{
    [Fact]
    public void Binding_UsesRenderedUvInsteadOfGeographicXy()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        float[] vertices = [
            -100, -100, 0, 1, 1, 1, 0,
             100, -100, 0, 1, 1, 1, 0,
               0,  100, 0, 1, 1, 1, 0];
        var mesh = RenderMesh.FromOpaqueTerrain("label-uv", vertices);
        var samples = new short[RenderDemTile.Side * RenderDemTile.Side];
        samples[1801 * RenderDemTile.Side + 1800] = 100;
        var draw = new RenderDraw(mesh, Matrix4x4.Identity, true, false)
        {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, samples),
            DemUV = new float[] { .49f, .49f, .51f, .49f, .502f, .51f }
        };
        var label = new GeoContourLabel(0, 0, 0, 0, 0, 0, 0, 1, 0, 23, 120);
        var bound = Assert.Single(session.BindContourLabelMeshes([draw], [label], 1280, 720));
        Assert.InRange(bound.East, -10.1f, -9.9f);
        Assert.InRange(bound.North, -0.1f, 0.1f);
    }
    [Fact]
    public void BindingUsesGradientOfGpuDemCellForTangent()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        var mesh = RenderMesh.FromOpaqueTerrain("gradient", new float[] {
            -100,-100,0,1,1,1,0, 100,-100,0,1,1,1,0, 0,100,0,1,1,1,0 });
        short[] samples = new short[RenderDemTile.Side * RenderDemTile.Side];
        int cell = 1800 * RenderDemTile.Side + 1800;
        samples[cell] = 0; samples[cell + 1] = -30;
        samples[cell + RenderDemTile.Side] = 100;
        samples[cell + RenderDemTile.Side + 1] = 70;
        var draw = new RenderDraw(mesh, Matrix4x4.Identity, true, false) {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, samples),
            DemUV = new float[] { .49f,.51f,.51f,.51f,.5f,.49f } };
        var label = new GeoContourLabel(0, 0, 0, 0, 0, 0, 0, 1, 0, 23, 120);
        var bound = Assert.Single(session.BindContourLabelMeshes([draw], [label], 1280, 720));
        Assert.InRange(Math.Abs(bound.TangentNorth), .25f, .35f);
    }
}
