using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Runtime;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E603C02GeographicRepresentation
{
    [Fact]
    public async Task GlobalProxyAndNoneShareActualRenderAndPickState()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07Cube(cpu,
            new GeoCoordinate(23.5, 120.5, 0), 8, out var cube).Status);
        Guid id = cube!.Id;
        var position = wb.World.GetSpatialPosition(id);
        source.Camera.FrameTop(position.ToGeo(), 50);
        var frame = source.G07RenderFrame(800, 600);
        Assert.Equal(id, Pick());
        wb.Scene.SetRepresentation(id, EntityRepresentationKind.Proxy);
        Assert.Equal("builtin/cube-proxy", Assert.Single(wb.Scene.RenderGeographic(frame)).Mesh.AssetId);
        Assert.Equal(id, Pick());
        wb.StartSimulation();
        Assert.Contains(wb.G07EnvironmentDraws(source, cpu, 800, 600, frame),
            draw => draw.Mesh.AssetId == "builtin/cube-proxy");
        wb.StopSimulation();
        wb.Scene.SetRepresentation(id, EntityRepresentationKind.None);
        Assert.Empty(wb.Scene.RenderGeographic(frame));
        Assert.Null(Pick());
        Assert.Equal(position, wb.World.GetSpatialPosition(id));
        wb.Scene.SetRepresentation(id, EntityRepresentationKind.Mesh);
        Assert.Equal(id, Pick());
        Guid? Pick() => GeoPreviewMeshPicker.Pick(wb.Scene, cpu, frame, new(400, 300), 800, 600);
    }
}