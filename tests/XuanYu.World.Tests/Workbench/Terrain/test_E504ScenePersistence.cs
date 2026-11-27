using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Persistence;

namespace XuanYu.World.Tests;

public sealed class test_E504ScenePersistence
{
    [Fact]
    public void TerrainRoundTrip_ShouldPreserveSamplesAndSceneIdentity()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(120f, 80f, 0.5f);
        var field = wb.TerrainMap.HeightField;
        field.SetHeight(5, 7, 3.25f);
        Guid terrainId = Assert.Single(wb.Scene.Objects).Id;
        string path = Path.Combine(Path.GetTempPath(),
            $"XYE-E504-{Guid.NewGuid():N}.xyscene");
        try
        {
            SceneFile.Save(wb.Scene, path);
            var reopened = new Workbench(use3D: true);
            SceneFile.Load(reopened.Scene, path);
            Assert.Equal(terrainId, Assert.Single(reopened.Scene.Objects).Id);
            Assert.Same(reopened.Scene, reopened.TerrainScene);
            Assert.Equal(120f, reopened.TerrainMap.WidthMeters);
            Assert.Equal(80f, reopened.TerrainMap.LengthMeters);
            Assert.Equal(0.5f, reopened.TerrainMap.HeightField.SpacingMeters);
            Assert.Equal(3.25f, reopened.TerrainMap.HeightField.GetHeight(5, 7));
            Assert.Single(reopened.ViewportDraws(800, 600), draw =>
                draw.Mesh.AssetId.StartsWith("terrain/local/", StringComparison.Ordinal));
        }
        finally { File.Delete(path); }
    }
}