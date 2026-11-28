using System.Numerics;
using Xunit;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_Preview3D
{
    [Fact]
    public void Cube_ShouldHaveSixFacesAndRealDepth()
    {
        var scene = PreviewSeed.Create3D();
        var cube = Assert.Single(scene.Objects);
        Assert.Equal(36, cube.Mesh.VertexCount);
        Assert.Equal(12, cube.Mesh.TriangleCount);
        Assert.Equal(cube.Id, Assert.Single(scene.World.Entities).Id);
        float[] values = cube.Mesh.Vertices.ToArray();
        var points = Enumerable.Range(0, cube.Mesh.VertexCount)
            .Select(i => new Vector3(values[i * 7], values[i * 7 + 1],
                values[i * 7 + 2])).ToArray();
        Assert.Contains(points, p => p.Z < -0.6f);
        Assert.Contains(points, p => p.Z > 0.6f);
    }

    [Fact]
    public void Viewport_ShouldUsePerspectiveAndAspectRatio()
    {
        var scene = PreviewSeed.Create3D();
        var draw = Assert.Single(scene.ViewportDraws(800, 600));
        Assert.NotEqual(Matrix4x4.Identity, draw.ModelToClip);
        Vector4 origin = Vector4.Transform(
            new Vector4(0f, 0f, 0f, 1f), draw.ModelToClip);
        Assert.True(origin.W > 0f);
        Assert.InRange(origin.Z / origin.W, 0f, 1f);
        Assert.InRange(MathF.Abs(origin.X / origin.W), 0f, 0.001f);
        var wide = Assert.Single(scene.ViewportDraws(1200, 600));
        Assert.NotEqual(draw.ModelToClip.M11, wide.ModelToClip.M11);
    }

    [Fact]
    public void ViewportPicking_ShouldUseSameCameraAsRendering()
    {
        var scene = PreviewSeed.Create3D();
        var cube = Assert.Single(scene.Objects);
        Guid? hit = PreviewMeshPicker.Pick(scene, 400, 300, 800, 600);
        Assert.Equal(cube.Id, hit);
    }

    [Fact]
    public void WorldTransform_ShouldChangeViewportMatrix()
    {
        var workbench = new Workbench(use3D: true);
        var cube = Assert.Single(workbench.Scene.Objects);
        var before = Assert.Single(workbench.Scene.ViewportDraws(800, 600));
        workbench.World.SetTransform(cube.Id, new Transform(
            new Vector3(2.5f, 0f, 0f), Quaternion.Identity, Vector3.One));
        var after = Assert.Single(workbench.Scene.ViewportDraws(800, 600));
        Assert.NotEqual(before.ModelToClip, after.ModelToClip);
    }
}