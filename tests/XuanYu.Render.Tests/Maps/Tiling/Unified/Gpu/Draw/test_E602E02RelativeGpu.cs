using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;
using Xunit;

namespace XuanYu.World.Tests.Maps;

[Collection("VulkanHardware")]
public sealed class test_E602E02RelativeGpu
{
    [Fact]
    public void ActualTerrainShaderUsesDoubleRenderOrigin()
    {
        var tile = new GeoUnifiedTile(0, 33806, 12768);
        var anchor = new EnuFrame(new GeoCoordinate(0, 0, 0));
        var geo = new GeoCoordinate((tile.South + 16) / 3600d - 90d,
            (tile.West + 16) / 3600d - 180d, 21);
        var local = new EnuFrame(geo);
        var eye = anchor.ToEnu(local.ToEcef(new(0, 0, 1200)));
        var p = anchor.ToEnu(local.OriginEcef);
        var forward = new Vector3((float)(p.East - eye.East), (float)(p.North - eye.North), (float)(p.Up - eye.Up));
        var north = anchor.ToEnu(local.ToEcef(new(0, 1, 0)));
        var up = new Vector3((float)(north.East - p.East), (float)(north.North - p.North), (float)(north.Up - p.Up));
        var mvp = Matrix4x4.CreateLookAt(Vector3.Zero, forward, up) *
            Matrix4x4.CreateOrthographic(2000, 2000, 1, 3000);
        var upload = new TerrainPoolUpload(1, new short[1089], default,
            Enumerable.Repeat((short)2100, 1089).ToArray());
        var draw = new TerrainPoolDraw(1, tile.South, tile.West, 1, anchor.Origin, mvp);
        var topology = Enumerable.Range(0, 16).Select(GeoUnifiedGridIndices.Create).ToArray();
        using var context = new VkContext();
        byte[] visible = context.ProbeTerrainViewport(upload, draw with { RenderOrigin = eye }, topology, []);
        byte[] absent = context.ProbeTerrainViewport(upload, draw, topology, []);
        int i = (64 * 128 + 64) * 4;
        Assert.InRange((int)visible[i], 21, 25);
        Assert.InRange((int)visible[i + 1], 80, 84);
        Assert.InRange((int)visible[i + 2], 24, 28);
        Assert.InRange((int)absent[i], 7, 11);
        Assert.InRange((int)absent[i + 1], 25, 29);
        Assert.InRange((int)absent[i + 2], 54, 58);
    }
}