using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E03C02Gizmo
{
    private static Matrix4x4 Projection(Vector3 eye)
    {
        var p = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3, 4f / 3, .1f, 10000);
        p.M22 = -p.M22;
        return Matrix4x4.CreateLookAt(eye, Vector3.Zero, Vector3.UnitZ) * p;
    }
    private static Vector2 Project(Vector3 point, Matrix4x4 matrix) =>
        GeoGizmoLayout.Screen(Vector4.Transform(new Vector4(point, 1), matrix), 800, 600);

    [Fact]
    public void RealProjectedAxisAndPlaneConstrainMotion()
    {
        var m = Projection(new(200, -300, 200));
        var layout = GeoGizmoLayout.Create(m, 800, 600)!;
        var east = Assert.Single(layout.Handles, h => h.Id == GeoGizmoHandle.E);
        Assert.Equal(GeoGizmoHandle.E, layout.Pick(east.Points[1]));
        var origin = Project(Vector3.Zero, m);
        var axis = GeoGizmoDrag.Begin(m, 800, 600, GeoGizmoHandle.E, origin)!;
        Assert.True(axis.TryDelta(Project(new(25, 0, 0), m), out var e));
        Assert.InRange(e.East, 24.95, 25.05);
        Assert.Equal(0d, e.North); Assert.Equal(0d, e.Up);
        var plane = GeoGizmoDrag.Begin(m, 800, 600, GeoGizmoHandle.EN, origin)!;
        Assert.True(plane.TryDelta(Project(new(20, -30, 0), m), out var en));
        Assert.InRange(en.East, 19.95, 20.05); Assert.InRange(en.North, -30.05, -29.95);
        Assert.Equal(0d, en.Up);
    }

    [Fact]
    public void OffscreenAndParallelHandlesAreUnavailable()
    {
        var m = Projection(new(0, -300, 0));
        var layout = GeoGizmoLayout.Create(m, 800, 600)!;
        Assert.DoesNotContain(layout.Handles, h => h.Id == GeoGizmoHandle.N || h.Id == GeoGizmoHandle.EN);
        Assert.Null(GeoGizmoDrag.Begin(m, 800, 600, GeoGizmoHandle.N, layout.Center));
        Assert.Null(GeoGizmoLayout.Create(Matrix4x4.CreateTranslation(3, 0, 0), 800, 600));
    }

    [Fact]
    public async Task GroundDragUsesOriginalAndDoesNotWriteOnMissingCoverage()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        wb.SwitchTo(PageId.World);
        source.Camera.FrameTop(new(23.5, 120.5, 0), 300_000d);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07InspectionCube(cpu, out var created).Status);
        var cube = Assert.IsType<PreviewObject>(created);
        var before = wb.World.Find(cube.Id)!.Transform;
        Assert.Equal(GeoSampleStatus.Valid, wb.DragG07Entity(cpu, cube.Id, before, new(100, 0, 0), true));
        var first = wb.World.Find(cube.Id)!.Transform;
        Assert.Equal(GeoSampleStatus.Valid, wb.DragG07Entity(cpu, cube.Id, before, new(100, 0, 0), true));
        Assert.Equal(first, wb.World.Find(cube.Id)!.Transform);
        Assert.InRange(source.G07EntityHeights(cpu, first.Spatial.ToGeo()).AboveGround!.Value, 99.99, 100.01);
        Assert.Equal(GeoSampleStatus.OutsideCoverage, wb.DragG07Entity(cpu, cube.Id, before, new(200000, 0, 0), true));
        Assert.Equal(first, wb.World.Find(cube.Id)!.Transform);
        Assert.Equal(GeoSampleStatus.Valid, wb.DragG07Entity(cpu, cube.Id, before, default, true));
        Assert.Equal(before, wb.World.Find(cube.Id)!.Transform);
    }
}