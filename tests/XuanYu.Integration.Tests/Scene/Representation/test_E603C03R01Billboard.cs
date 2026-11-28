using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Persistence;
using XuanYu.Render;
using XuanYu.Runtime;
using XuanYu.World;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_E603C03R01Billboard
{
    [Fact]
    public void LocalTopViewAndZoomKeepPixelSizeWhileMetersRespondToZoom()
    {
        var scene = new PreviewScene { Camera = new ViewportCamera() };
        var item = scene.CreateLogicEntity();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(path, """
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10"/></svg>
                """);
            scene.BindSymbol(item.Id, path);
            var camera = scene.Camera!;
            camera.SnapAxis(Vector3.UnitZ);
            var before = Assert.Single(scene.ViewportDraws(800,600));
            Assert.Equal(64f, PixelWidth(before), 3);
            camera.Orbit(1000,0);
            camera.Zoom(2);
            var after = Assert.Single(scene.ViewportDraws(800,600));
            Assert.Equal(64f, PixelWidth(after), 3);
            Assert.Equal(0f, after.ModelToClip.M12);
            Assert.Equal(0f, after.ModelToClip.M21);
            scene.SetSymbolStyle(item.Id, new(2,false));
            float metersBefore = PixelWidth(Assert.Single(scene.ViewportDraws(800,600)));
            camera.Zoom(2);
            Assert.True(PixelWidth(Assert.Single(scene.ViewportDraws(800,600))) > metersBefore);
            var transform = scene.World.Find(item.Id)!.Transform;
            scene.SetTransform(item.Id, transform with { Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 2f) });
            var rotated = Assert.Single(scene.ViewportDraws(800,600));
            Assert.Equal(0f, rotated.ModelToClip.M12);
            Assert.Equal(0f, rotated.ModelToClip.M21);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void PointRoundTripsInsteadOfDiscardingRepresentation()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xyscene");
        try
        {
            var scene = new PreviewScene();
            var item = scene.CreateLogicEntity();
            SceneFile.Save(scene,path);
            scene.SetRepresentation(item.Id, EntityRepresentationKind.Point);
            SceneFile.Save(scene,path);
            var restored = new PreviewScene();
            SceneFile.Load(restored,path);
            Assert.Equal(EntityRepresentationKind.Point, restored.GetRepresentation(item.Id));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CreateUnitUsesActualLocalSymbolAndSharesResource()
    {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.svg");
        try
        {
            var wb = new Workbench(use3D: true);
            int count = wb.Scene.Objects.Count;
            Assert.Throws<FileNotFoundException>(() => wb.CreateLocalUnit(path));
            Assert.Equal(count, wb.Scene.Objects.Count);
            File.WriteAllText(path, """
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 10 10"><rect width="10" height="10"/></svg>
                """);
            var first = wb.CreateLocalUnit(path);
            var second = wb.CreateLocalUnit(path);
            Assert.Equal("单位_001", first.Name);
            Assert.Equal("单位_002", second.Name);
            Assert.False(second.IsGeographic);
            Assert.Null(second.Mesh);
            Assert.Equal(second.Id, wb.Scene.SelectedId);
            Assert.Equal(EntityRepresentationKind.Symbol, wb.Scene.GetRepresentation(second.Id));
            Assert.Same(wb.Scene.GetActiveMesh(first.Id), wb.Scene.GetActiveMesh(second.Id));
            Assert.Equal(1, wb.Scene.SvgAssets.LoadedCount);
            Assert.Equal(2, wb.ViewportDraws(800, 600).Count(d => d.IsBillboard));
        }
        finally { File.Delete(path); }
    }

    private static float PixelWidth(RenderDraw draw) =>
        draw.ModelToClip.M11 / draw.ModelToClip.M44 * 400;
}