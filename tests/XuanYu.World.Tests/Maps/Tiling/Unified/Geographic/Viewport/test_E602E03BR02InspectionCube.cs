using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E602E03BR02InspectionCube
{
    [Fact]
    public async Task InspectionCubeUsesRealGroundSizeSelectionAndFocus()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        var geoid = Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!);
        source.ConfigureG07Geoid(geoid);
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        wb.SwitchTo(PageId.World);
        source.Camera.FrameTop(new(23.5, 120.5, 0), 300_000d);
        var sample = wb.CreateG07InspectionCube(cpu, out var created);
        Assert.Equal(GeoSampleStatus.Valid, sample.Status);
        var cube = Assert.IsType<PreviewObject>(created);
        Assert.True(cube.IsGeographic);
        Assert.Equal<Guid?>(cube.Id, wb.Scene.SelectedId);
        var transform = wb.World.Find(cube.Id)!.Transform;
        Assert.Equal(SpatialSpace.Ecef, transform.Spatial.Space);
        var geo = transform.Spatial.ToGeo();
        Assert.Equal(23.5d, geo.LatitudeDegrees, 8);
        Assert.Equal(120.5d, geo.LongitudeDegrees, 8);
        Assert.Equal(100d, source.G07EntityHeights(cpu, geo).AboveGround!.Value, 5);
        Assert.Equal(transform.Scale.X, transform.Scale.Y);
        Assert.Equal(transform.Scale.X, transform.Scale.Z);
        var vertices = cube.Mesh.Vertices.ToArray();
        float min = float.PositiveInfinity, max = float.NegativeInfinity;
        for (int i = 0; i < vertices.Length; i += 7)
        {
            min = Math.Min(min, vertices[i]);
            max = Math.Max(max, vertices[i]);
        }
        Assert.InRange((max - min) * transform.Scale.X, 199.999f, 200.001f);
        Assert.Equal(geo.LatitudeDegrees, source.Camera.Target.LatitudeDegrees, 8);
        Assert.Equal(geo.LongitudeDegrees, source.Camera.Target.LongitudeDegrees, 8);
        Assert.InRange(source.Camera.DistanceMeters, 600d, 800d);
        wb.SetSkyEnabled(false);
        wb.SetWorldGridEnabled(false);
        var frame = source.G07RenderFrame(800, 600);
        var draw = Assert.Single(wb.G07EnvironmentDraws(source, cpu, 800, 600, frame));
        Assert.Same(cube.Mesh, draw.Mesh);
        Assert.True(draw.Visible);
        var center = Vector4.Transform(new Vector4(0, 0, 0, 1), draw.ModelToClip);
        Assert.InRange(MathF.Abs(center.X / center.W), 0f, 1e-4f);
        Assert.InRange(MathF.Abs(center.Y / center.W), 0f, 1e-4f);
        Assert.Equal<Guid?>(cube.Id, GeoPreviewMeshPicker.Pick(wb.Scene, cpu, frame, new(400, 300), 800, 600));
    }

    [Fact]
    public async Task OutsideCoverageDoesNotFallBackToLocalCube()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        wb.SwitchTo(PageId.World);
        var local = wb.Scene.CreateCube();
        int count = wb.Scene.Objects.Count;
        source.Camera.FrameTop(new(23.5, 119.5, 0), 300_000d);
        var sample = wb.CreateG07InspectionCube(cpu, out var cube);
        Assert.Equal(GeoSampleStatus.OutsideCoverage, sample.Status);
        Assert.Null(cube);
        Assert.Equal(count, wb.Scene.Objects.Count);
        Assert.Equal(count, wb.World.Entities.Count);
        Assert.Equal<Guid?>(local.Id, wb.Scene.SelectedId);
        Assert.Equal(SpatialSpace.Local, wb.World.GetSpatialPosition(local.Id).Space);
        Assert.Equal(Vector3.One, wb.World.Find(local.Id)!.Transform.Scale);
    }
}