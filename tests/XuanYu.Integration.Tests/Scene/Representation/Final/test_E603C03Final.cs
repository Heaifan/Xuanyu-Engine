using System.Numerics;
using System.Text;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C03Final
{
    [Fact]
    public void NearSymbolOverlaysAndFarSideIsInvisible()
    {
        var scene = new PreviewScene();
        var near = scene.CreateLogicEntity(SpatialPosition.FromGeo(new GeoCoordinate(0,0,100)));
        var far = scene.CreateLogicEntity(SpatialPosition.FromGeo(new GeoCoordinate(0,180,100)));
        scene.SetRepresentation(near.Id, EntityRepresentationKind.Point);
        scene.SetRepresentation(far.Id, EntityRepresentationKind.Point);
        var anchor = new EnuFrame(new GeoCoordinate(0,0,0));
        var frame = new GeoRenderFrame(anchor, anchor.ToEcef(new(0,-1000,1_000_000)), anchor.OriginEcef, 1, 1, 20_000_000);
        var draws = scene.RenderGeographic(frame, 800,800);
        var visible = Assert.Single(draws, d => d.Visible);
        Assert.True(visible.OutlineThroughOcclusion);
        Assert.True(visible.IsBillboard);
        Assert.False(frame.IsAboveHorizon(scene.World.GetSpatialPosition(far.Id).ToEcef()));
    }

    [Fact]
    public void ExplicitConversionAndHistoryPreserveIdentityAndPosition()
    {
        var scene = new PreviewScene();
        var local = SpatialPosition.FromLocal(20,30,40);
        var unit = scene.CreateLogicEntity(local);
        var upper = scene.CreateLogicEntity();
        scene.SetOrganizationParent(unit.Id,upper.Id);
        var anchor = new GeoCoordinate(23,121,100);
        scene.SetRepresentation(unit.Id,EntityRepresentationKind.Point);
        scene.ConvertUnitSpace(unit.Id,anchor);
        var global = scene.World.GetSpatialPosition(unit.Id);
        Assert.Equal(SpatialSpace.Ecef,global.Space);
        Assert.Equal(upper.Id,scene.World.GetOrganizationParent(unit.Id));
        scene.UndoTransform();
        Assert.Equal(local,scene.World.GetSpatialPosition(unit.Id));
        scene.UndoTransform();
        Assert.Equal(EntityRepresentationKind.None,scene.GetRepresentation(unit.Id));
        scene.RedoTransform(); scene.RedoTransform();
        Assert.Equal(global,scene.World.GetSpatialPosition(unit.Id));
        scene.ConvertUnitSpace(unit.Id,anchor);
        var p = scene.World.GetSpatialPosition(unit.Id);
        Assert.Equal(local.X,p.X,6); Assert.Equal(local.Y,p.Y,6); Assert.Equal(local.Z,p.Z,6);
    }

    [Theory]
    [InlineData("M10 50 C10 10 90 10 90 50 S10 90 10 50 Z", .45f)]
    [InlineData("M10 50 Q50 0 90 50 T10 50 Z", .55f)]
    [InlineData("M10 50 A40 30 0 1 1 90 50 A40 30 0 1 1 10 50 Z", .45f)]
    public void CurvesProduceActualGeometryAndFillAlpha(string data, float expectedMaxX)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"""
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><g transform="translate(5,0)"><path d="{data}" fill-opacity="0.5"/></g></svg>
            """));
        var mesh = SvgMeshReader.Read("test/curves",stream);
        Assert.True(mesh.TriangleCount>4);
        var v = mesh.Vertices.ToArray();
        Assert.All(Enumerable.Range(0,mesh.VertexCount), i => Assert.Equal(.5f,v[i*7+6]));
        Assert.InRange(Enumerable.Range(0,mesh.VertexCount).Max(i=>v[i*7]), expectedMaxX - .01f, expectedMaxX + .01f);
    }
}