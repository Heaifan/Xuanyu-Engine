using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E03C03Orientation
{
    private static GeoGizmoLayout Layout(GeoGizmoMode mode)
    {
        var p = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 3, 4f / 3, .1f, 10000);
        p.M22 = -p.M22;
        return GeoGizmoLayout.Create(Matrix4x4.CreateLookAt(new(200, -300, 200),
            Vector3.Zero, Vector3.UnitZ) * p, 800, 600, mode)!;
    }

    [Fact]
    public void RingUsesSignedContinuousAngleAndSnap()
    {
        var layout = Layout(GeoGizmoMode.Rotate);
        var ring = Assert.Single(layout.Handles, h => h.Id == GeoGizmoHandle.U);
        var drag = new GeoGizmoOrientationDrag(layout, ring.Id, ring.Points[0]);
        Assert.InRange(drag.Value(ring.Points[16], false, false).Z, 89.99f, 90.01f);
        Assert.InRange(drag.Value(ring.Points[32], false, false).Z, 179.99f, 180.01f);
        Assert.InRange(drag.Value(ring.Points[48], false, false).Z, 269.99f, 270.01f);
        var snapped = drag.Value(ring.Points[49], false, true);
        Assert.Equal(new Vector3(0, 0, 275), snapped);
    }

    [Fact]
    public void ScaleUsesOnlySelectedAxisAndUniformCenter()
    {
        var layout = Layout(GeoGizmoMode.Scale);
        var east = Assert.Single(layout.Handles, h => h.Id == GeoGizmoHandle.E);
        var axis = east.Points[1] - layout.Center;
        var drag = new GeoGizmoOrientationDrag(layout, east.Id, east.Points[1]);
        Assert.Equal(new Vector3(2, 1, 1), drag.Value(east.Points[1] + axis, false, true));
        Assert.Equal(GeoGizmoHandle.All, layout.Pick(layout.Center));
        var uniform = new GeoGizmoOrientationDrag(layout, GeoGizmoHandle.All, layout.Center);
        Assert.Equal(new Vector3(2), uniform.Value(layout.Center - new Vector2(0, 80), false, false));
        Assert.Equal(new Vector3(.01f), uniform.Value(layout.Center + new Vector2(0, 800), false, false));
    }

    [Fact]
    public async Task OrientationDragKeepsAuthorityAndUsesOriginal()
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
        var original = wb.World.Find(cube.Id)!.Transform;
        wb.DragG07Orientation(cube.Id, original, GeoGizmoMode.Rotate, new(0, 0, 90));
        var rotated = wb.World.Find(cube.Id)!.Transform;
        Assert.Equal(original.Spatial, rotated.Spatial);
        Assert.InRange(Vector3.Transform(Vector3.UnitX, rotated.Rotation).Y, .999f, 1.001f);
        wb.DragG07Orientation(cube.Id, original, GeoGizmoMode.Rotate, new(0, 0, 90));
        Assert.Equal(rotated, wb.World.Find(cube.Id)!.Transform);
        wb.DragG07Orientation(cube.Id, rotated, GeoGizmoMode.Scale, new(2, 1, 1));
        var scaled = wb.World.Find(cube.Id)!.Transform;
        Assert.Equal(rotated.Spatial, scaled.Spatial);
        Assert.Equal(rotated.Rotation, scaled.Rotation);
        Assert.Equal(rotated.Scale * new Vector3(2, 1, 1), scaled.Scale);
        wb.DragG07Orientation(cube.Id, rotated, GeoGizmoMode.Scale, Vector3.One);
        Assert.Equal(rotated, wb.World.Find(cube.Id)!.Transform);
    }
}