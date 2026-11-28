using XuanYu.Core.Geodesy;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World.Maps;
using XuanYu.Runtime;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_E603C03AGeoLogicEntity
{
    [Fact]
    public async Task LogicEntityGroundingFocusAndOutsideCoverage()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        wb.SwitchTo(PageId.World);
        var point = new GeoCoordinate(23.5, 120.5, 0);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07LogicEntity(cpu, point, 50d, out var created).Status);
        var item = Assert.IsType<PreviewObject>(created);
        Assert.Equal(item.Id, wb.Scene.SelectedId);
        Assert.Null(item.Mesh);
        Assert.True(wb.FocusG07Entity());
        Assert.Equal(23.5d, source.Camera.Target.LatitudeDegrees, 8);
        Assert.Equal(120.5d, source.Camera.Target.LongitudeDegrees, 8);
        Assert.Equal(GeoSampleStatus.Valid, wb.GroundG07Entity(cpu, item.Id));
        var position = wb.World.GetSpatialPosition(item.Id).ToGeo();
        Assert.Equal(0d, source.G07EntityHeights(cpu, position).AboveGround!.Value, 5);
        Assert.Empty(wb.Scene.RenderGeographic(source.G07RenderFrame(800, 600)));
        int count = wb.Scene.Objects.Count;
        Assert.Equal(GeoSampleStatus.OutsideCoverage,
            wb.CreateG07LogicEntity(cpu, new(23.5, 119.5, 0), 0d, out var missing).Status);
        Assert.Null(missing);
        Assert.Equal(count, wb.Scene.Objects.Count);
        Assert.Equal(item.Id, wb.Scene.SelectedId);
    }

    [Fact]
    public async Task C03R01UnitCreatesGeographicSymbolAtGroundOffset()
    {
        using var fixture = new E602CHgtFixture();
        string path = fixture.Create("N23E120");
        string svg = Path.Combine(Path.GetDirectoryName(path)!, "infantry.svg");
        File.WriteAllText(svg, """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10"/></svg>
            """);
        var wb = new Workbench(use3D: true);
        wb.ImportTerrain(path);
        var source = wb.G07PreviewSource!;
        source.ConfigureG07Geoid(Egm96GeoidGrid.Load(Environment.GetEnvironmentVariable("XYE_EGM96_GRID")!));
        using var cpu = await source.LoadG07DatasetAsync(path, 512);
        wb.SwitchTo(PageId.World);
        var point = new GeoCoordinate(23.5, 120.5, 0);
        Assert.Equal(GeoSampleStatus.Valid, wb.CreateG07Unit(cpu, point, svg, out var created).Status);
        var item = Assert.IsType<PreviewObject>(created);
        Assert.True(item.IsGeographic);
        Assert.Equal("单位_001", item.Name);
        Assert.Equal(item.Id, wb.Scene.SelectedId);
        Assert.Equal(EntityRepresentationKind.Symbol, wb.Scene.GetRepresentation(item.Id));
        var position = wb.World.GetSpatialPosition(item.Id).ToGeo();
        Assert.Equal(100d, source.G07EntityHeights(cpu, position).AboveGround!.Value, 5);
        var draw = Assert.Single(wb.Scene.RenderGeographic(source.G07RenderFrame(800, 600), 800, 600));
        Assert.True(draw.IsBillboard);
        Assert.Equal(64f, draw.ModelToClip.M11 / draw.ModelToClip.M44 * 400, 3);
        int count = wb.Scene.Objects.Count;
        Assert.Equal(GeoSampleStatus.OutsideCoverage,
            wb.CreateG07Unit(cpu, new(23.5, 119.5, 0), svg, out var missing).Status);
        Assert.Null(missing);
        Assert.Equal(count, wb.Scene.Objects.Count);
    }
}