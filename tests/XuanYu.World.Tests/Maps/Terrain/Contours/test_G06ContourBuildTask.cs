using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06ContourBuildTask
{
    [Fact]
    public void IncrementalOutput_MatchesFullBuildAndHasMajorLines()
    {
        var anchor = new EnuFrame(new GeoCoordinate(24.5d, 121.5d, 0d));
        float[] triangle = [
            0f, 0f, -100f, 0.6f, 0.6f, 0.6f, 1f,
            100f, 0f, 500f, 0.6f, 0.6f, 0.6f, 1f,
            0f, 100f, -100f, 0.6f, 0.6f, 0.6f, 1f
        ];
        var terrain = new RenderMesh("test/contours", [..triangle, ..triangle]);
        var style = new GeoContourStyle(100, 0.25f, 4f, 0.6f);
        RenderMesh? full = GeoContourMesh.Build(terrain, anchor, style);
        var task = new GeoContourBuildTask(terrain, anchor, style);
        Assert.False(task.Advance(1));
        Assert.True(task.Advance(1));
        RenderMesh? progressive = task.Finish();
        Assert.NotNull(full);
        Assert.NotNull(progressive);
        Assert.Equal(full.Vertices.ToArray(), progressive.Vertices.ToArray());
        ReadOnlySpan<float> data = progressive.Vertices.Span;
        var widths = new List<float>();
        for (int i = 0; i < data.Length; i += 42)
        {
            Vector3 a = new(data[i], data[i + 1], data[i + 2]);
            Vector3 b = new(data[i + 35], data[i + 36], data[i + 37]);
            widths.Add(Vector3.Distance(a, b));
        }
        Assert.True(widths.Count >= 5);
        Assert.True(widths.Max() > widths.Min() * 1.6f);
    }
}