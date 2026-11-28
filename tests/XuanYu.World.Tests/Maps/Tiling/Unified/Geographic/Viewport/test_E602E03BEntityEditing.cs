using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E03BEntityEditing
{
    [Fact]
    public async Task EditGroundAndOrthometricPositionPreservesOrientationAndIdentity()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (1800, 1800, 100));
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
        source.ConfigureG07Geoid(geoid);
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07Cube(cpu, new(23.25, 120.25, 0), 0.7, out var cube).Status);
        Guid id = cube!.Id;
        var rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.4f);
        var scale = new Vector3(2, 3, 4);
        wb.Scene.SetTransform(id, wb.World.Find(id)!.Transform with { Rotation = rotation, Scale = scale });
        Assert.Equal(GeoSampleStatus.Valid, wb.MoveG07Entity(cpu, id, new(23.5, 120.5, 3), true));
        var current = wb.World.Find(id)!.Transform;
        var heights = source.G07EntityHeights(cpu, current.Spatial.ToGeo());
        Assert.Equal(103d, heights.Orthometric, 5);
        Assert.Equal(3d, heights.AboveGround!.Value, 5);
        Assert.Equal(rotation, current.Rotation);
        Assert.Equal(scale, current.Scale);
        Assert.Equal(GeoSampleStatus.OutsideCoverage, wb.MoveG07Entity(cpu, id, new(23.5, 119.5, 3), true));
        Assert.Equal(current, wb.World.Find(id)!.Transform);
        Assert.Equal(GeoSampleStatus.Valid, wb.MoveG07Entity(cpu, id, new(23.5, 120.5, 123), false));
        var next = wb.World.Find(id)!.Transform;
        Assert.Equal(123d + geoid.UndulationMeters(new(23.5, 120.5, 0)), next.Spatial.ToGeo().HeightMeters, 5);
        wb.Scene.Select(id);
        wb.SwitchTo(PageId.World);
        Assert.True(wb.FocusG07Entity());
        Assert.Equal(next, wb.World.Find(id)!.Transform);
        Assert.Equal(23.5, source.Camera.Target.LatitudeDegrees, 8);
        Assert.Equal(120.5, source.Camera.Target.LongitudeDegrees, 8);
    }

    [Fact]
    public async Task GeographicPickUsesDepthAndTerrainOcclusion()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        var point = new GeoCoordinate(23.5, 120.5, 0);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07Cube(cpu, point, 8, out var front).Status);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07Cube(cpu, point, 2, out var back).Status);
        var target = wb.World.GetSpatialPosition(front!.Id).ToGeo();
        source.Camera.FrameTop(target, 50);
        var frame = source.G07RenderFrame(800, 600);
        Assert.Equal(front.Id, Pick());
        wb.Scene.SetVisible(front.Id, false);
        Assert.Equal(back!.Id, Pick());
        Assert.Equal(GeoSampleStatus.Valid, wb.MoveG07Entity(cpu, back.Id, new(23.5, 120.5, -20), false));
        Assert.Null(Pick());
        Guid? Pick() => GeoPreviewMeshPicker.Pick(wb.Scene, cpu, frame, new(400, 300), 800, 600);
    }
}