using System.Numerics;
using Xunit;
using XuanYu.Editor.TerrainEditing;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests;

public sealed class test_E5MR01D02TerrainShape
{
    [Fact]
    public void Peak_LowContourHasUnequalRadialExtent()
    {
        var field = new TerrainHeightField(201, 201, 1f);
        var stroke = new TerrainSculptStroke(field, new TerrainBrushSettings(80f, 0.5f, 0.5f),
            false, naturalPeakHeight: 60f, naturalPeakSeed: 7429);
        Assert.True(stroke.MoveTo(new(100, 100)) > 0);
        stroke.FinishMountain();
        int[] radii = new int[8];
        for (int i = 0; i < radii.Length; i++)
        {
            double angle = i * Math.PI / 4d;
            for (int r = 1; r <= 80; r++)
            {
                int x = 100 + (int)Math.Round(r * Math.Cos(angle));
                int y = 100 + (int)Math.Round(r * Math.Sin(angle));
                if (field.GetHeight(x, y) >= 3f) radii[i] = r;
            }
        }
        Assert.True(radii.Min() > 0);
        Assert.True(radii.Max() - radii.Min() >= 8,
            $"3m等高线方向半径：{string.Join(",", radii)}");
    }

    [Fact]
    public void StraightStroke_HasOffsetCrestsAndVariableFootprint()
    {
        var field = new TerrainHeightField(241, 181, 1f);
        var stroke = new TerrainSculptStroke(field, new TerrainBrushSettings(40f, 0.5f, 0.5f),
            false, naturalPeakHeight: 60f, naturalPeakSeed: 7429);
        stroke.MoveTo(new(40, 90)); stroke.MoveTo(new(200, 90));
        Assert.True(stroke.FinishMountain() > 0);
        int minWidth = field.SamplesY, maxWidth = 0, offsetColumns = 0;
        for (int x = 60; x <= 180; x++)
        {
            int width = 0, crest = 90;
            for (int y = 50; y <= 130; y++)
            {
                if (field.GetHeight(x, y) >= 5f) width++;
                if (field.GetHeight(x, y) > field.GetHeight(x, crest)) crest = y;
            }
            if (Math.Abs(crest - 90) >= 4) offsetColumns++;
            minWidth = Math.Min(minWidth, width); maxWidth = Math.Max(maxWidth, width);
            Assert.True(width > 0);
            Assert.Equal(0f, field.GetHeight(x, 50));
            Assert.Equal(0f, field.GetHeight(x, 130));
        }
        Assert.True(offsetColumns >= 20, $"偏移山脊列数={offsetColumns}");
        Assert.True(maxWidth - minWidth >= 8, $"5m等高线宽度差={maxWidth - minWidth}m");
    }

    [Fact]
    public void FirstClick_UsesSameReliefAsZeroLengthRidge()
    {
        var a = new TerrainHeightField(81, 81, 1f);
        var b = new TerrainHeightField(81, 81, 1f);
        var reference = new TerrainHeightField(81, 81, 1f);
        var brush = new TerrainBrushSettings(20f, 0.5f, 0.5f);
        Vector2 p = new(40, 40);
        var stroke = new TerrainSculptStroke(a, brush, false, naturalPeakHeight: 30f);
        stroke.MoveTo(p);
        var stamp = new TerrainNaturalRidgeStamp(b, reference, brush, 30f, 7429, p);
        stamp.Apply(p, p);
        Assert.Equal(a.Samples.ToArray(), b.Samples.ToArray());
        Assert.Equal(30f, a.GetHeight(40, 40));
        Assert.Equal(0, stroke.FinishMountain());
    }
}