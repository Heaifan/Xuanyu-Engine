using System;
using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;

namespace XuanYu.World.Tests;

public sealed class test_E504CBrushPreview
{
    [Fact]
    public void Falloff_UsesExpectedWeights()
    {
        var brush = new TerrainBrushSettings(10f, 0.5f, 0.6f);
        Assert.Equal(1f, brush.WeightAt(0f));
        Assert.Equal(1f, brush.WeightAt(3f));
        Assert.InRange(brush.WeightAt(brush.HalfWeightRadius), 0.499f, 0.501f);
        Assert.Equal(0f, brush.WeightAt(10f));
        Assert.Equal(0f, brush.WeightAt(12f));
        var hard = new TerrainBrushSettings(10f, 1f, 0f);
        Assert.Equal(1f, hard.WeightAt(9f));
        Assert.Equal(0f, hard.WeightAt(10f));
    }

    [Fact]
    public void Outline_FollowsSurfaceAndClosesOnValidTerrain()
    {
        var center = new Vector2(20f, 30f);
        var lines = TerrainBrushOutline.Build(center, 5f, 1f,
            p => p.X * 0.5f + p.Y);
        Assert.NotEmpty(lines);
        Assert.Equal(lines[0].Start, lines[^1].End);
        foreach (var line in lines)
        {
            AssertPoint(line.Start, center);
            AssertPoint(line.End, center);
        }
    }

    [Fact]
    public void Outline_DoesNotBridgeInvalidSurface()
    {
        var lines = TerrainBrushOutline.Build(Vector2.Zero, 5f, 1f,
            p => MathF.Abs(p.X) > 1f ? 0f : null);
        Assert.NotEmpty(lines);
        foreach (var line in lines)
        {
            Assert.True(MathF.Abs(line.Start.X) > 1f);
            Assert.True(MathF.Abs(line.End.X) > 1f);
            Assert.Equal(MathF.Sign(line.Start.X), MathF.Sign(line.End.X));
        }
        Assert.Empty(TerrainBrushOutline.Build(Vector2.Zero, 5f, 1f, _ => null));
    }

    private static void AssertPoint(Vector3 point, Vector2 center)
    {
        float distance = Vector2.Distance(new Vector2(point.X, point.Y), center);
        Assert.InRange(distance, 4.999f, 5.001f);
        Assert.InRange(point.Z - (point.X * 0.5f + point.Y), -0.001f, 0.001f);
    }
}