using Xunit;
using XuanYu.Editor.Pages;

namespace XuanYu.World.Tests;

public sealed class test_E504LocalTerrain
{
    [Fact]
    public void Launch3D_ShouldCreateFlatTerrainInsteadOfCube()
    {
        var wb = new Workbench(use3D: true);
        Assert.Equal(100f, wb.TerrainMap.WidthMeters);
        Assert.Equal(100f, wb.TerrainMap.LengthMeters);
        Assert.Equal(101, wb.TerrainMap.HeightField.SamplesX);
        Assert.Equal(101, wb.TerrainMap.HeightField.SamplesY);
        Assert.All(wb.TerrainMap.HeightField.Samples.ToArray(),
            height => Assert.Equal(0f, height));
        Assert.Single(wb.Scene.Objects);
        Assert.Equal("Terrain_01", wb.Scene.Objects[0].Name);
        Assert.Same(wb.Scene, wb.TerrainScene);
        Assert.Same(wb.Scene.Camera, wb.TerrainScene.Camera);
    }

    [Fact]
    public void Resize_ShouldKeepSceneAndChangeRealHeightField()
    {
        var wb = new Workbench(use3D: true);
        var scene = wb.Scene;
        var camera = scene.Camera;
        wb.ResizeLocalTerrain(120f, 80f, 0.5f);
        Assert.Same(scene, wb.TerrainScene);
        Assert.Same(camera, wb.TerrainScene.Camera);
        Assert.Equal(241, wb.TerrainMap.HeightField.SamplesX);
        Assert.Equal(161, wb.TerrainMap.HeightField.SamplesY);
        Assert.Equal(120f, wb.TerrainMap.WidthMeters);
        Assert.Equal(80f, wb.TerrainMap.LengthMeters);
        Assert.Single(wb.Scene.Objects);
    }

    [Theory]
    [InlineData(400f, 400f, 1f, 201, 201, 2f)]
    [InlineData(1000f, 1000f, 1f, 201, 201, 5f)]
    [InlineData(2000f, 1000f, 1f, 201, 101, 10f)]
    public void LargeTerrain_AutomaticallyAdjustsSampling(
        float width, float length, float desired,
        int samplesX, int samplesY, float actualSpacing)
    {
        var wb = new Workbench(use3D: true);
        var scene = wb.Scene;
        wb.ResizeLocalTerrain(width, length, desired);
        var field = wb.TerrainMap.HeightField;
        Assert.Equal(samplesX, field.SamplesX);
        Assert.Equal(samplesY, field.SamplesY);
        Assert.Equal(actualSpacing, field.SpacingXMeters);
        Assert.Equal(actualSpacing, field.SpacingYMeters);
        Assert.Equal(width, wb.TerrainMap.WidthMeters);
        Assert.Equal(length, wb.TerrainMap.LengthMeters);
        Assert.Same(scene, wb.TerrainScene);
        Assert.Single(scene.Objects);
    }

    [Fact]
    public void NonDivisibleSize_PreservesRequestedBoundary()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(501f, 302f, 3f);
        var field = wb.TerrainMap.HeightField;
        Assert.InRange(field.WidthMeters, 500.999f, 501.001f);
        Assert.InRange(field.LengthMeters, 301.999f, 302.001f);
        Assert.InRange(field.SamplesX, 2, 257);
        Assert.InRange(field.SamplesY, 2, 257);
    }

    [Fact]
    public void EditMode_ShouldNotWriteHeightBeforeBrushIsImplemented()
    {
        var wb = new Workbench(use3D: true);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        Assert.True(wb.IsTerrainEditMode);
        Assert.Equal(0f, wb.TerrainMap.HeightField.GetHeight(50, 50));
        wb.SetTerrainMode(TerrainWorkspaceMode.Object);
        Assert.False(wb.IsTerrainEditMode);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            wb.ResizeLocalTerrain(0f, 400f, 1f));
    }
}