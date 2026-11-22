using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G06LabelReal(ITestOutputHelper output)
{
    [G06GeometryFact]
    public void RealN23E120_ProducesBoundLabelAndGap()
    {
        string path = Environment.GetEnvironmentVariable("XYE_G06_R04_HGT")!;
        var session = new GeoTerrainRenderSession(NasademHgtFile.Load(path));
        session.ToggleNative();
        session.Camera.FrameTop(new GeoCoordinate(23.5, 120.85, 0), 5000);
        RenderDraw[] draws = [];
        GeoContourLabel[] bound = [];
        Assert.True(SpinWait.SpinUntil(() =>
        {
            draws = session.Draws(1280, 720).ToArray();
            if (session.PendingCount > 0) return false;
            var candidates = session.VisibleContourLabels(1280, 720);
            bound = session.BindContourLabelMeshes(draws, candidates, 1280, 720);
            return bound.Length >= 2;
        }, TimeSpan.FromSeconds(30)), "真实 N23E120 未能产生两个绑定标注");
        var result = session.AttachContourLabelGaps(draws, bound, 1280, 720, 1,
            out var visible, _ => 30);
        output.WriteLine($"候选绑定={bound.Length}, 实际显示={visible.Length}, Draw={draws.Length}");
        Assert.True(visible.Length >= 2);
        var label = visible[0];
        var gap = Assert.Single(result, d => ReferenceEquals(d.Mesh, label.Owner)).ContourLabelGap;
        Assert.Equal(label.ElevationMeters, gap.W);
        Assert.InRange(gap.X, 0, 1280);
        Assert.InRange(gap.Y, 0, 720);
        var timer = Stopwatch.StartNew();
        for (int i = 0; i < 20; i++)
            session.AttachContourLabelGaps(draws, visible, 1280, 720, 1, out _, _ => 30);
        output.WriteLine($"稳定显示 {visible.Length} 个标签，{draws.Length} 个 Draw，平均 {timer.Elapsed.TotalMilliseconds / 20:F2} ms/帧");
    }
    [G06GeometryFact]
    public void AdjacentRealTiles_NativeOnAndOffBindOnlyMatchingDem()
    {
        string west = Environment.GetEnvironmentVariable("XYE_G06_R04_HGT")!;
        string east = Path.Combine(Path.GetDirectoryName(west)!, "NASADEM_HGT_n23e121.zip");
        Assert.True(File.Exists(east), "缺少相邻 N23E121 数据");
        var session = new GeoTerrainRenderSession(NasademHgtFile.Load(west), NasademHgtFile.Load(east));
        session.Camera.FrameTop(new GeoCoordinate(23.5, 121.0, 0), 5000);
        foreach (bool native in new[] { true, false })
        {
            if (session.NativeEnabled != native) session.ToggleNative();
            GeoContourLabel[] visible = [];
            Assert.True(SpinWait.SpinUntil(() =>
            {
                var draws = session.Draws(1280, 720).ToArray();
                if (session.PendingCount > 0 || (native && session.NativeDrawCount == 0)) return false;
                var labels = session.BindContourLabelMeshes(draws,
                    session.VisibleContourLabels(1280, 720), 1280, 720);
                session.AttachContourLabelGaps(draws, labels, 1280, 720, 1, out visible, _ => 30);
                return visible.Length > 0;
            }, TimeSpan.FromSeconds(30)), $"Native={native} 时无可见标注");
            Assert.All(visible, label => Assert.InRange(label.West, 120, 121));
            output.WriteLine($"Native={native}, 标注数={visible.Length}, Native Draw={session.NativeDrawCount}");
        }
    }
}
