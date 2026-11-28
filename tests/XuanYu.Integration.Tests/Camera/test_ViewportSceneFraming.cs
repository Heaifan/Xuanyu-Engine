using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Preview;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_ViewportSceneFraming
{
    [Fact]
    public void Home_ShouldFrameAllVisibleGeometry()
    {
        var scene = PreviewSeed.Create3D();
        var camera = scene.Camera!;
        var second = scene.Add("Cube_002", PreviewCubeMesh.Create(),
            Matrix4x4.CreateTranslation(12f, 0f, 0f));
        camera.Orbit(70, 40);
        camera.Pan(80, 10, 580);

        Assert.True(ViewportSceneFraming.FrameAll(camera, scene.Objects, 1.3f));

        Assert.InRange(camera.Target.X, 5.999f, 6.001f);
        Assert.InRange(camera.Target.Y, -0.001f, 0.001f);
        Vector3 expected = Vector3.Normalize(new ViewportCamera().Position);
        Vector3 actual = Vector3.Normalize(camera.Position - camera.Target);
        Assert.InRange(Vector3.Distance(expected, actual), 0f, 0.0001f);
        Assert.Equal(second.Id, scene.Objects[1].Id);
    }

    [Fact]
    public void Focus_ShouldReadLatestWorldTransform()
    {
        var scene = PreviewSeed.Create3D();
        var camera = scene.Camera!;
        var cube = scene.Objects[0];
        scene.Select(cube.Id);
        camera.Orbit(50, -20);
        Vector3 direction = Vector3.Normalize(camera.Position - camera.Target);
        scene.World.SetTransform(cube.Id, new Transform(
            new Vector3(2.5f, 0f, 1f), Quaternion.Identity, Vector3.One));

        Assert.True(ViewportSceneFraming.Focus(camera, scene.Selected, 1.3f));

        Assert.InRange(camera.Target.X, 2.499f, 2.501f);
        Assert.InRange(camera.Target.Z, 0.999f, 1.001f);
        Assert.InRange(Vector3.Distance(direction,
            Vector3.Normalize(camera.Position - camera.Target)), 0, 0.0001f);
    }

    [Fact]
    public void Home_ShouldIgnoreHiddenObjects()
    {
        var scene = PreviewSeed.Create3D();
        var camera = scene.Camera!;
        var hidden = scene.Add("Hidden", PreviewCubeMesh.Create(),
            Matrix4x4.CreateTranslation(100f, 0f, 0f));
        scene.SetVisible(hidden.Id, false);

        Assert.True(ViewportSceneFraming.FrameAll(camera, scene.Objects, 1.3f));
        Assert.InRange(camera.Target.X, -0.001f, 0.001f);
    }

    [Fact]
    public void Focus_WithoutSelection_ShouldLeaveViewUnchanged()
    {
        var scene = PreviewSeed.Create3D();
        var camera = scene.Camera!;
        Matrix4x4 before = camera.ViewProjection(800, 600);

        Assert.False(ViewportSceneFraming.Focus(camera, scene.Selected, 1.3f));
        Assert.Equal(before, camera.ViewProjection(800, 600));
    }

    [Fact]
    public void Home_EmptyScene_ShouldResetDefaultView()
    {
        var scene = new PreviewScene();
        var camera = new ViewportCamera();
        camera.Orbit(50, 30);
        camera.Zoom(5);

        Assert.False(ViewportSceneFraming.FrameAll(camera, scene.Objects, 1.3f));
        Assert.Equal(new ViewportCamera().Position, camera.Position);
    }
}