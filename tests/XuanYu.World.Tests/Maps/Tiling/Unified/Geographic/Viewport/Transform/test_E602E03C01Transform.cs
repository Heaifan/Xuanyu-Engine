using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E03C01Transform
{
    [Fact]
    public async Task MoveRotateScaleAndGroundUseOneAuthoritativePosition()
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
        var initial = wb.World.Find(cube.Id)!.Transform;
        var frame = new EnuFrame(initial.Spatial.ToGeo());
        wb.TranslateG07Entity(cube.Id, new(0.01d, 100d, 200d));
        var moved = wb.World.Find(cube.Id)!.Transform;
        var delta = frame.ToEnu(moved.Spatial.ToEcef());
        Assert.Equal(0.01d, delta.East, 6);
        Assert.Equal(100d, delta.North, 6);
        Assert.Equal(200d, delta.Up, 6);
        Assert.Equal(initial.Rotation, moved.Rotation);
        Assert.Equal(initial.Scale, moved.Scale);
        wb.RotateG07Entity(cube.Id, new(90f, 0f, 0f));
        wb.ScaleG07Entity(cube.Id, new(1f, 2f, 3f));
        var changed = wb.World.Find(cube.Id)!.Transform;
        Assert.Equal(moved.Spatial, changed.Spatial);
        Assert.InRange(Vector3.Distance(Vector3.Transform(Vector3.UnitY, changed.Rotation), Vector3.UnitZ), 0f, 1e-5f);
        Assert.Equal(initial.Scale * new Vector3(1, 2, 3), changed.Scale);
        Assert.Equal(GeoSampleStatus.Valid, wb.GroundG07Entity(cpu, cube.Id));
        var grounded = wb.World.Find(cube.Id)!.Transform;
        var geo = grounded.Spatial.ToGeo();
        Assert.Equal(moved.Spatial.ToGeo().LatitudeDegrees, geo.LatitudeDegrees, 8);
        Assert.Equal(moved.Spatial.ToGeo().LongitudeDegrees, geo.LongitudeDegrees, 8);
        Assert.Equal(changed.Rotation, grounded.Rotation);
        Assert.Equal(changed.Scale, grounded.Scale);
        // 绕东轴转90°后，原模型 Y 轴的400m尺寸成为垂直尺寸。
        Assert.InRange(source.G07EntityHeights(cpu, geo).AboveGround!.Value, 199.99d, 200.01d);
    }

    [Fact]
    public async Task OutsideCoverageGroundAndInvalidScalePreserveEntity()
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
        wb.TranslateG07Entity(cube.Id, new(200_000d, 0d, 0d));
        var before = wb.World.Find(cube.Id)!.Transform;
        Assert.Equal(GeoSampleStatus.OutsideCoverage, wb.GroundG07Entity(cpu, cube.Id));
        Assert.Throws<ArgumentException>(() => wb.ScaleG07Entity(cube.Id, new(1f, 0f, 1f)));
        Assert.Equal(before, wb.World.Find(cube.Id)!.Transform);
    }
}