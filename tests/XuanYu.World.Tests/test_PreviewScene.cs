
using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview;
using XuanYu.Render;

namespace XuanYu.World.Tests;

public sealed class test_PreviewScene
{
    [Fact]
    public void Mesh_AcceptsTriangleQuadAndCubeTriangleLists()
    {
        Assert.Equal(3, Mesh(3).VertexCount);
        Assert.Equal(6, Mesh(6).VertexCount);
        Assert.Equal(36, Mesh(36).VertexCount);
    }

    [Fact]
    public void Scene_SelectsByIdRegardlessOfMeshSize()
    {
        var scene = new PreviewScene();
        PreviewObject a = scene.Add(
            "三角形", Mesh(3), Matrix4x4.Identity);
        PreviewObject b = scene.Add(
            "立方体", Mesh(36), Matrix4x4.Identity);

        scene.Select(b.Id);
        Assert.Equal(b.Id, scene.SelectedId);
        Assert.False(scene.Draws[0].Selected);
        Assert.True(scene.Draws[1].Selected);

        scene.Select(a.Id);
        Assert.True(scene.Draws[0].Selected);
    }

    [Fact]
    public void Visibility_OnlyChangesTargetObject()
    {
        var scene = new PreviewScene();
        PreviewObject a = scene.Add(
            "A", Mesh(3), Matrix4x4.Identity);
        PreviewObject b = scene.Add(
            "B", Mesh(6), Matrix4x4.Identity);

        scene.SetVisible(b.Id, false);

        Assert.True(scene.Draws[0].Visible);
        Assert.False(scene.Draws[1].Visible);
        Assert.True(a.Visible);
    }

    [Fact]
    public void RepeatedSelectionDoesNotEmitExtraChange()
    {
        var scene = new PreviewScene();
        PreviewObject item = scene.Add(
            "A", Mesh(3), Matrix4x4.Identity);

        int count = 0;
        scene.Changed += () => count++;

        scene.Select(item.Id);
        scene.Select(item.Id);

        Assert.Equal(1, count);
    }

    private static RenderMesh Mesh(int vertexCount) =>
        new($"test/{vertexCount}", new float[vertexCount * 7]);
}
