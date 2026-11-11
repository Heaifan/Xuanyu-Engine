
using System.Numerics;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using Xunit;

namespace XuanYu.World.Tests;

public sealed class test_ViewportPixelPicking
{
    [Theory]
    [InlineData(756, 580)]
    [InlineData(1134, 870)]
    [InlineData(1512, 1160)]
    [InlineData(1200, 800)]
    public void PickingTracksPhysicalClientPixels(int width, int height)
    {
        var scene = new PreviewScene();
        float[] vertices =
        [
            -0.6f, -0.6f, 0f, 1f, 0f, 0f, 1f,
             0.6f, -0.6f, 0f, 0f, 1f, 0f, 1f,
             0f,    0.6f, 0f, 0f, 0f, 1f, 1f
        ];
        var target = scene.Add("分辨率测试对象",
            new RenderMesh("test/viewport-pixels", vertices),
            Matrix4x4.Identity);

        var center = PreviewMeshPicker.Pick(
            scene, width / 2, height / 2, (uint)width, (uint)height);
        var blank = PreviewMeshPicker.Pick(
            scene, width / 12, height / 12, (uint)width, (uint)height);

        Assert.Equal(target.Id, center);
        Assert.Null(blank);
    }
}
