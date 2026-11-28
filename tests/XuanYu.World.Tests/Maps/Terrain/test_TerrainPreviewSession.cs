using System.Numerics;
using Xunit;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class TestTerrainPreviewSession
{
    [Fact]
    public void LargeMap_PickingReturnsUnscaledMeters()
    {
        var field = new TerrainHeightField(2, 2, 2000f);
        var map = new MapDocument("比例测试", field);
        var preview = new TerrainPreviewSession(map);

        Assert.InRange(preview.DisplayScale, 0.1279f, 0.1281f);
        Assert.True(preview.TryPick(new Vector2(400, 300),
            800, 600, out var hit));
        Assert.InRange(hit.WorldPosition.X, 999f, 1001f);
        Assert.InRange(hit.WorldPosition.Y, 999f, 1001f);
        Assert.InRange(hit.WorldPosition.Z, -0.01f, 0.01f);
    }

    [Fact]
    public void AnisotropicMap_PickingUsesBothAxes()
    {
        var field = new TerrainHeightField(2, 2, 2000f, 4000f);

        Assert.Equal(2000f, field.WidthMeters);
        Assert.Equal(4000f, field.LengthMeters);
        Assert.Equal(new Vector3(2000f, 4000f, 0f),
            field.GetWorldPosition(1, 1));

        var preview = new TerrainPreviewSession(
            new MapDocument("双轴比例测试", field));

        Assert.True(preview.TryPick(new Vector2(400, 300),
            800, 600, out var hit));

        // 保留大范围地形原有的双轴比例验收容差。
        Assert.InRange(
            hit.WorldPosition.X / field.WidthMeters,
            0.499f, 0.501f);
        Assert.InRange(
            hit.WorldPosition.Y / field.LengthMeters,
            0.499f, 0.501f);
        Assert.InRange(hit.WorldPosition.Z, -0.01f, 0.01f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void AnisotropicMap_ProjectedPointPicksBothAxes(int mode)
    {
        var field = new TerrainHeightField(2, 2, 2000f, 4000f);
        var preview = new TerrainPreviewSession(new MapDocument("双轴离中心拾取", field));
        var camera = preview.Scene.Camera!;
        if (mode == 1) camera.ToggleProjection();
        if (mode == 2) camera.SnapAxis(Vector3.UnitZ);

        var point = new Vector3(1300f, 1700f, 0f);
        var clip = Vector4.Transform(new Vector4(point * preview.DisplayScale, 1f),
            camera.ViewProjection(800, 600));
        var pixel = new Vector2((clip.X / clip.W + 1f) * 400f,
            (clip.Y / clip.W + 1f) * 300f);

        Assert.True(preview.TryPick(pixel, 800, 600, out var hit));
        Assert.InRange(hit.WorldPosition.X, point.X - 0.05f, point.X + 0.05f);
        Assert.InRange(hit.WorldPosition.Y, point.Y - 0.05f, point.Y + 0.05f);
        Assert.InRange(hit.WorldPosition.Z, -0.01f, 0.01f);
    }

    [Theory]
    [InlineData(25f)]
    [InlineData(50f)]
    [InlineData(75f)]
    public void FlatGrid_ObliqueRayHitsVertexAtExactSurfaceHeight(float x)
    {
        var field = new TerrainHeightField(101, 101, 100f);
        var origin = new Vector3(141.05f, -93.06f, 83.23f);
        var target = new Vector3(x, 50f, 0f);
        Assert.True(TerrainHeightRayPicker.TryPick(field, origin, target - origin, out var hit));
        Assert.InRange(hit.WorldPosition.X, x - 0.001f, x + 0.001f);
        Assert.InRange(hit.WorldPosition.Y, 49.999f, 50.001f);
        Assert.Equal(0f, hit.WorldPosition.Z);
    }
}