using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E505AViewportSculpt
{
    [Theory]
    [InlineData(false, 0.5f)]
    [InlineData(true, -0.5f)]
    public void Stroke_UpdatesFieldMeshAndPickingWithoutReplacingEntity(bool lower, float expected)
    {
        var wb = CreateWorkbench();
        var scene = wb.TerrainScene;
        var camera = scene.Camera!;
        var terrain = Assert.Single(scene.Objects);
        var before = terrain.Mesh!;
        var transform = terrain.ModelToClip;
        var pixel = Project(Vector3.Zero, camera.ViewProjection(800, 600));
        Assert.True(wb.TryBeginTerrainSculpt(pixel, 800, 600, lower));
        wb.EndTerrainSculpt();
        var field = wb.TerrainMap.HeightField;
        Assert.InRange(field.GetHeight(50, 50), expected - 0.001f, expected + 0.001f);
        Assert.Same(terrain, Assert.Single(scene.Objects));
        Assert.Same(camera, scene.Camera);
        Assert.Equal(transform, terrain.ModelToClip);
        Assert.Equal(before.AssetId, terrain.Mesh!.AssetId);
        Assert.NotSame(before, terrain.Mesh);
        Assert.Contains(scene.Draws, draw => ReferenceEquals(draw.Mesh, terrain.Mesh));
        var vertices = terrain.Mesh!.Vertices.Span;
        bool found = false;
        for (int i = 0; i < vertices.Length; i += 7)
        {
            if (vertices[i] != 50f || vertices[i + 1] != 50f) continue;
            Assert.InRange(vertices[i + 2], expected - 0.001f, expected + 0.001f);
            found = true;
        }
        Assert.True(found);
        var updatedPixel = Project(new Vector3(0f, 0f, expected),
            camera.ViewProjection(800, 600));
        var hit = wb.PickTerrainPoint(updatedPixel, 800, 600);
        Assert.True(hit.HasValue);
        Assert.InRange(hit.GetValueOrDefault().Z, expected - 0.01f, expected + 0.01f);
        var preview = wb.BuildTerrainBrushPreview(updatedPixel, 800, 600);
        Assert.NotNull(preview);
        Assert.InRange(preview!.CenterMeters.Z, expected - 0.01f, expected + 0.01f);
    }

    [Fact]
    public void ObjectModeAndMiss_CannotStartStroke()
    {
        var wb = CreateWorkbench();
        Assert.False(wb.TryBeginTerrainSculpt(new Vector2(-1, 300), 800, 600, false));
        wb.SetTerrainMode(TerrainWorkspaceMode.Object);
        Assert.False(wb.TryBeginTerrainSculpt(new Vector2(400, 300), 800, 600, false));
        Assert.All(wb.TerrainMap.HeightField.Samples.ToArray(), h => Assert.Equal(0f, h));
    }

    [Fact]
    public void MissBreaksPath_AndReleaseStopsFurtherWrites()
    {
        var wb = CreateWorkbench();
        var camera = wb.TerrainScene.Camera!.ViewProjection(800, 600);
        var left = Project(new Vector3(-25, 0, 0), camera);
        var right = Project(new Vector3(25, 0, 0), camera);
        Assert.True(wb.TryBeginTerrainSculpt(left, 800, 600, false));
        wb.MoveTerrainSculpt(new Vector2(-1, 300), 800, 600);
        wb.MoveTerrainSculpt(right, 800, 600);
        wb.EndTerrainSculpt();
        wb.MoveTerrainSculpt(Project(Vector3.Zero, camera), 800, 600);
        var field = wb.TerrainMap.HeightField;
        Assert.True(field.GetHeight(25, 50) > 0f);
        Assert.True(field.GetHeight(75, 50) > 0f);
        Assert.Equal(0f, field.GetHeight(50, 50));
    }

    private static Workbench CreateWorkbench()
    {
        var wb = new Workbench(use3D: true);
        wb.ResizeLocalTerrain(100f, 100f, 1f);
        wb.SwitchTo(PageId.Terrain);
        wb.SetTerrainMode(TerrainWorkspaceMode.Edit);
        wb.SetTerrainBrush(new TerrainBrushSettings(10f, 0.5f, 0.5f));
        return wb;
    }

    private static Vector2 Project(Vector3 point, Matrix4x4 camera)
    {
        var clip = Vector4.Transform(new Vector4(point, 1f), camera);
        return new Vector2((clip.X / clip.W + 1f) * 400f,
            (clip.Y / clip.W + 1f) * 300f);
    }
}