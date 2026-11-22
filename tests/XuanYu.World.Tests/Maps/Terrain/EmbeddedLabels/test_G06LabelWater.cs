using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06LabelWater
{
    [Fact]
    public void WaterSurfaceRejectsLabel()
    {
        using var hgt = new G02BHgtFixture("N23E120");
        var session = new GeoTerrainRenderSession(hgt.Load());
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.5, 0), 1000);
        var mesh = RenderMesh.FromOpaqueTerrain("water", new float[] {
            -100,-100,0,1,1,1,0, 100,-100,0,1,1,1,0, 0,100,0,1,1,1,0 });
        var water = new short[903 * 903];
        Array.Fill(water, (short)-1);
        water[0] = water[1] = water[2] = 1;
        var draw = new RenderDraw(mesh, Matrix4x4.Identity, true, false) {
            ContourIntervalMeters = 100, ContourOpacity = 1,
            ResidentDem = new(23, 120, new short[RenderDemTile.Side * RenderDemTile.Side]),
            DemUV = new float[] { .49f,.49f,.51f,.49f,.5f,.51f },
            WaterAtlas = new(23, 120, 903, water, 1) };
        var label = new GeoContourLabel(0, 0, 0, 0, 0, 0, 0, 1, 0, 23, 120);
        Assert.Empty(session.BindContourLabelMeshes([draw], [label], 1280, 720));
    }
}
