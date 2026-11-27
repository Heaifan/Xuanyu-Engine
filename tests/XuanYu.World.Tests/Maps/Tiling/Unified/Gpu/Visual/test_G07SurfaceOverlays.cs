using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_G07SurfaceOverlays
{
    [Fact]
    public void OverlayTogglesChangePixelsWithoutReuploadingHeight()
    {
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.2f);
        var draw = new TerrainPoolDraw(1, 324000, 648000, 1, origin, mvp);
        var style = new TerrainVisualStyle(GeoTerrainRenderSession.G07ElevationPalette, false);
        var heights = Enumerable.Range(0, 1089).Select(i => (short)((i % 33) * 20)).ToArray();
        var contours = style with { Contours = new Vector4(100, 0.3f, 100, 0) };
        uint gapShape = (uint)BitConverter.HalfToUInt16Bits((Half)(MathF.PI / 2)) |
            ((uint)BitConverter.HalfToUInt16Bits((Half)34f) << 16);
        var gap = new Vector4(91, 64, BitConverter.UInt32BitsToSingle(gapShape), 500);
        TerrainVisualStyle[] styles = [style,
            contours,
            style with { Grid = new TerrainGridStyle(true, 8d / 3600, 8d / 3600, 2, 2) },
            style with { Selection = new Vector4(0, 0, 16f / 3600f, 1) },
            contours with { LabelGaps = [gap] }, style];
        TerrainPoolFrame[] frames = styles.Select((item, i) => new TerrainPoolFrame(
            i == 0 ? [new TerrainPoolUpload(1, heights)] : [], [1], [], [],
            [draw with { Style = item }])).ToArray();
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        var result = context.ProbeTerrainPagePool(2, 1, 16 * 1024 * 1024, topology, frames);
        for (int i = 1; i <= 3; i++)
        {
            var pixels = result.Frames[i].Pixels;
            Assert.Contains(Enumerable.Range(0, pixels.Length), j => pixels[j] != result.Frames[0].Pixels[j]);
        }
        Assert.Contains(Enumerable.Range(0, result.Frames[4].Pixels.Length),
            j => result.Frames[4].Pixels[j] != result.Frames[1].Pixels[j]);
        Assert.Equal(result.Frames[0].Pixels, result.Frames[5].Pixels);
        Assert.All(result.Frames, frame => Assert.Equal(1, frame.TotalUploadedPages));
    }

    [Fact]
    public void GeographicGrid_ExtendsBeyondOldTwelveCellWindow()
    {
        var origin = new GeoCoordinate(16d / 3600, 16d / 3600, 0);
        var mvp = Matrix4x4.CreateScale(0.0015f, 0.0015f, 0.0001f) *
            Matrix4x4.CreateTranslation(0, 0, 0.2f);
        var style = new TerrainVisualStyle(GeoTerrainRenderSession.G07ElevationPalette, false);
        var draw = new TerrainPoolDraw(1, 324000, 648000, 1, origin, mvp) { Style = style };
        var grid = style with { Grid = new TerrainGridStyle(true, 1d / 3600, 1d / 3600, 16, 16) };
        TerrainPoolFrame[] frames = [
            new([new TerrainPoolUpload(1, new short[1089])], [1], [], [], [draw]),
            new([], [1], [], [], [draw with { Style = grid }])];
        int[][] topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        var result = context.ProbeTerrainPagePool(2, 1, 16 * 1024 * 1024, topology, frames);
        var before = result.Frames[0].Pixels;
        var after = result.Frames[1].Pixels;
        Assert.Contains(Enumerable.Range(26, 14), x => Changed(x));
        Assert.Contains(Enumerable.Range(88, 14), x => Changed(x));
        Assert.Equal(1, result.Frames[1].TotalUploadedPages);

        bool Changed(int x) => Enumerable.Range(55, 18).Any(y =>
            Enumerable.Range(0, 3).Any(c => before[(y * 128 + x) * 4 + c] !=
                after[(y * 128 + x) * 4 + c]));
    }
}