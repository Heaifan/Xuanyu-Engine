using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07AdaptiveGrid
{
    [Fact]
    public void ViewportHeightChangesMetricGridAndAdjacentLevelsMeet()
    {
        double distance = 200d * 600d / (60d * Math.Tan(Math.PI / 6d));
        var before = GridStep.ForViewport(distance * 0.9999d, 600);
        var after = GridStep.ForViewport(distance * 1.0001d, 600);
        Assert.Equal(100d, before.Step);
        Assert.Equal(200d, before.Next);
        Assert.Equal(200d, after.Step);
        Assert.InRange(before.Blend, 0.999f, 1f);
        Assert.InRange(after.Blend, 0f, 0.001f);
        Assert.True(GridStep.ForViewport(distance, 1200).Step < after.Step);
    }

    [Fact]
    public void RealShaderFadesDenseGridAndKeepsPageResident()
    {
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.2f);
        var style = new TerrainVisualStyle(GeoTerrainRenderSession.G07ElevationPalette, false);
        var draw = new TerrainPoolDraw(1, 324000, 648000, 1, origin, mvp);
        var dense = new TerrainGridStyle(true, 0.05d / 3600, 0.05d / 3600, 320, 320);
        var lower = new TerrainGridStyle(true, 4d / 3600, 4d / 3600, 4, 4);
        var upper = new TerrainGridStyle(true, 8d / 3600, 8d / 3600, 2, 2);
        TerrainVisualStyle[] styles = [style, style with { Grid = dense },
            style with { Grid = lower }, style with { Grid = lower with { NextRatio = 0.5d, Blend = 1f } },
            style with { Grid = upper }];
        TerrainPoolFrame[] frames = styles.Select((item, i) => new TerrainPoolFrame(
            i == 0 ? [new TerrainPoolUpload(1, new short[1089])] : [], [1], [], [],
            [draw with { Style = item }])).ToArray();
        using var context = new VkContext();
        var result = context.ProbeTerrainPagePool(2, 1, 16 * 1024 * 1024,
            Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray(), frames);
        var f = result.Frames;
        Assert.Equal(f[0].Pixels, f[1].Pixels);
        Assert.False(f[0].Pixels.AsSpan().SequenceEqual(f[2].Pixels));
        Assert.False(f[0].Pixels.AsSpan().SequenceEqual(f[4].Pixels));
        for (int i = 0; i < f[3].Pixels.Length; i++)
            Assert.InRange(Math.Abs(f[3].Pixels[i] - f[4].Pixels[i]), 0, 1);
        Assert.All(f, item => Assert.Equal(1, item.TotalUploadedPages));
    }
}