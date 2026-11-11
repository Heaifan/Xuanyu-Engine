using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_EditorCube
{
    [Fact]
    public void CreateCube_ShouldAppearInWorldTreeAndRender()
    {
        var scene = PreviewSeed.Create3D();
        PreviewObject original = Assert.Single(scene.Objects);

        PreviewObject created = scene.CreateCube();

        Assert.Equal("Cube_002", created.Name);
        Assert.NotEqual(original.Id, created.Id);
        Assert.Equal(2, scene.World.Entities.Count);
        Assert.Equal(2, scene.Objects.Count);
        Assert.Equal(created.Id, scene.SelectedId);
        Assert.Equal(36, created.Mesh.VertexCount);
        Assert.Equal(2, scene.ViewportDraws(800, 600).Count);
        Assert.Equal(1.9f, scene.World.Find(created.Id)!.Transform.Position.X);
    }

    [Fact]
    public void EditingTransform_ShouldUpdateWorldAndViewport()
    {
        var scene = PreviewSeed.Create3D();
        PreviewObject created = scene.CreateCube();
        var originalDraw = scene.ViewportDraws(800, 600)[1];
        var next = new Transform(
            new Vector3(2.5f, 0f, 1f),
            Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.4f),
            new Vector3(1.2f, 0.8f, 1.5f));

        scene.SetTransform(created.Id, next);

        Assert.Equal(next, scene.World.Find(created.Id)!.Transform);
        Assert.NotEqual(originalDraw.ModelToClip,
            scene.ViewportDraws(800, 600)[1].ModelToClip);
        Assert.Equal(created.Id, scene.SelectedId);
    }

    [Fact]
    public void DeleteAndCreate_ShouldNotReuseEntityIdentity()
    {
        var scene = PreviewSeed.Create3D();
        PreviewObject removed = scene.CreateCube();
        Assert.True(scene.Delete(removed.Id));

        PreviewObject created = scene.CreateCube();

        Assert.Equal("Cube_002", created.Name);
        Assert.NotEqual(removed.Id, created.Id);
        Assert.Equal(2, scene.World.Entities.Count);
    }
}