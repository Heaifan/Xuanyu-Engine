using System.Numerics;
using Xunit;
using XuanYu.Editor.Camera;

namespace XuanYu.World.Tests;

public sealed class test_E304CStandardViews
{
    [Theory]
    [InlineData(1f)]
    [InlineData(-1f)]
    public void VerticalAxis_IsExactAndViewProjectionFinite(float z)
    {
        var camera = new ViewportCamera();
        camera.SnapAxis(new Vector3(0, 0, z));
        Vector3 direction = Vector3.Normalize(camera.Position - camera.Target);
        Assert.InRange(Vector3.Distance(direction, new Vector3(0, 0, z)), 0, 0.0001f);
        Assert.InRange(Vector3.Distance(camera.ViewUp, Vector3.UnitY), 0, 0.0001f);
        Assert.True(camera.IsOrthographic);
        Matrix4x4 view = camera.ViewProjection(800, 600);
        foreach (float value in new[] { view.M11, view.M12, view.M13, view.M21,
            view.M22, view.M23, view.M31, view.M32, view.M33, view.M41, view.M42 })
            Assert.True(float.IsFinite(value));
        camera.Pan(20, -10, 600);
        Assert.True(float.IsFinite(camera.Target.X));
        Assert.True(float.IsFinite(camera.Target.Y));
        camera.Orbit(10, z > 0 ? -20 : 20);
        Assert.True(float.IsFinite(camera.Position.Z));
    }
}