using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E03AGeographicEntities
{
    [Fact]
    public async Task GroundedCubeEntersActualWorkbenchDrawListAndRetainsPosition()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
        source.ConfigureG07Geoid(geoid);
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        var local = wb.Scene.CreateCube();
        var point = new GeoCoordinate(23.5, 120.5, 0);
        var sample = wb.CreateG07Cube(cpu, point, 0.7d, out var created);
        Assert.Equal(GeoSampleStatus.Valid, sample.Status);
        var cube = Assert.IsType<XuanYu.Editor.Preview.PreviewObject>(created);
        var position = wb.World.GetSpatialPosition(cube.Id);
        Assert.Equal(SpatialSpace.Ecef, position.Space);
        Assert.InRange(Math.Abs(position.ToGeo().HeightMeters - geoid.UndulationMeters(point) - 0.7d), 0, 1e-5);
        Assert.Contains(cube, wb.Scene.Objects);
        Assert.Equal(SpatialSpace.Local, wb.World.GetSpatialPosition(local.Id).Space);
        wb.SetSkyEnabled(false);
        wb.SetTerrainGridEnabled(false);
        wb.SetWorldGridEnabled(false);
        source.Camera.FrameTop(position.ToGeo(), 100d);
        var frame = source.G07RenderFrame(800, 600);
        var draw = Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600, frame));
        Assert.Same(cube.Mesh, draw.Mesh);
        Assert.True(draw.Visible);
        var center = Vector4.Transform(new Vector4(0, 0, 0, 1), draw.ModelToClip);
        Assert.InRange(MathF.Abs(center.X / center.W), 0, 1e-5f);
        Assert.InRange(MathF.Abs(center.Y / center.W), 0, 1e-5f);
        wb.SwitchTo(PageId.World);
        source.Camera.Orbit(0.4d, -0.4d);
        var next = source.G07RenderFrame(800, 600);
        Assert.NotEqual(draw.ModelToClip, Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600, next)).ModelToClip);
        Assert.Equal(position, wb.World.GetSpatialPosition(cube.Id));
        wb.Scene.SetVisible(cube.Id, false);
        Assert.False(Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600, next)).Visible);
        Assert.True(wb.Scene.Delete(cube.Id));
        Assert.Empty(wb.G07EnvironmentDraws(source, cpu, 800, 600, next));
    }

    [Fact]
    public async Task MissingGroundDoesNotCreateEntityOrRenderBinding()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120", (1800, 1800, short.MinValue));
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        int count = wb.Scene.Objects.Count;
        var missing = wb.CreateG07Cube(cpu, new(23.5, 120.5, 0), 0.7d, out var cube);
        Assert.Equal(GeoSampleStatus.NoData, missing.Status);
        Assert.Null(cube);
        var outside = wb.CreateG07Cube(cpu, new(23.5, 119.5, 0), 0.7d, out cube);
        Assert.Equal(GeoSampleStatus.OutsideCoverage, outside.Status);
        Assert.Null(cube);
        Assert.Equal(count, wb.Scene.Objects.Count);
        Assert.Equal(count, wb.World.Entities.Count);
        Assert.Empty(wb.Scene.RenderGeographic(source.G07RenderFrame(800, 600)));
    }
}