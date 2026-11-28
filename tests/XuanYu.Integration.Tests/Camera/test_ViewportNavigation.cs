using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;

namespace XuanYu.World.Tests;

public sealed class test_ViewportNavigation
{
    [Theory]
    [InlineData(1f, 0f, 0f)]
    [InlineData(-1f, 0f, 0f)]
    [InlineData(0f, 1f, 0f)]
    [InlineData(0f, -1f, 0f)]
    [InlineData(0f, 0f, 1f)]
    [InlineData(0f, 0f, -1f)]
    public void SnapAxis_FacesRequestedDirection(float x, float y, float z)
    {
        var camera = new ViewportCamera();
        Vector3 axis = new(x, y, z);
        camera.SnapAxis(axis);
        Vector3 actual = Vector3.Normalize(camera.Position - camera.Target);
        Assert.True(Vector3.Dot(axis, actual) > 0.99f);
    }

    [Fact]
    public void ToggleProjection_ChangesRenderedProjection()
    {
        var camera = new ViewportCamera();
        Matrix4x4 before = camera.ViewProjection(800, 600);
        camera.ToggleProjection();
        Assert.True(camera.IsOrthographic);
        Assert.NotEqual(before, camera.ViewProjection(800, 600));
        camera.ToggleProjection();
        Assert.False(camera.IsOrthographic);
        Assert.Equal(before, camera.ViewProjection(800, 600));
    }
}