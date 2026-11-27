using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5M02CR01RidgeShape
{
    [Fact]
    public void LongRidge_HasPeakSaddleRecoveryAndUnevenSlopes()
    {
        var field = new TerrainHeightField(201, 61, 1f);
        var reference = new TerrainHeightField(201, 61, 1f);
        var stamp = new TerrainNaturalRidgeStamp(field, reference,
            new TerrainBrushSettings(20f, 1f, 0.5f), 100f, 7429, new Vector2(20, 30));
        stamp.Apply(new(20, 30), new(180, 30));
        float first = 0f, saddle = 100f, second = 0f, asymmetry = 0f;
        int minWidth = 61, maxWidth = 0;
        for (int x = 40; x <= 160; x++)
        {
            float h = field.GetHeight(x, 30);
            if (x <= 60) first = MathF.Max(first, h);
            if (x > 60 && x < 80) saddle = MathF.Min(saddle, h);
            if (x >= 80 && x <= 120) second = MathF.Max(second, h);
            Assert.InRange(h, 0.001f, 100f);
            int width = 0;
            for (int y = 0; y < field.SamplesY; y++)
                if (field.GetHeight(x, y) > 1f) width++;
            minWidth = Math.Min(minWidth, width);
            maxWidth = Math.Max(maxWidth, width);
            asymmetry = MathF.Max(asymmetry,
                MathF.Abs(field.GetHeight(x, 25) - field.GetHeight(x, 35)));
            Assert.Equal(0f, field.GetHeight(x, 10));
            Assert.Equal(0f, field.GetHeight(x, 50));
        }
        Assert.True(first - saddle >= 25f);
        Assert.True(second - saddle >= 40f);
        Assert.True(maxWidth - minWidth >= 4);
        Assert.True(asymmetry >= 10f);
    }
}