using System.Diagnostics;
using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Core.Preview.Terrain.Unified;
using XuanYu.Render.Vulkan;
using XuanYu.Render;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private sealed record G07FrameResult(string Profile, int Submitted, string Levels,
        int Drawn, int Resident, int Pending, int TotalPages, ulong Bytes, double Ms,
        bool Transition, bool Blocked, GeoContourLabel[] Labels, double LabelMs);

    private G07FrameResult DrawG07Coverage(VkSharedColorProbe probe,
        GeoUnifiedGpuSession session, VkTerrainPagePool pool, EnuPosition eye,
        Matrix4x4 mvp, uint width, uint height, IReadOnlyList<RenderDraw> environment, TerrainVisualStyle style,
        GeoTerrainRenderSession source, GeoUnifiedLodSession cpu, int interval, float scale)
    {
        long start = Stopwatch.GetTimestamp();
        var visible = cpu.CreateViewportVisibility(mvp);
        var frame = session.Step(eye, (int)height, Math.PI / 3, mvp, visible);
        int submitted = frame.SubmittedPages;
        double selectionMs = frame.SelectionMs;
        // 只在初始覆盖尚未可绘制时等待；正常 LOD 切换继续显示已提交覆盖。
        while (frame.Draws.Length == 0 && pool.PendingCount > 0)
        {
            pool.WaitForUpload();
            frame = session.Step(eye, (int)height, Math.PI / 3, mvp, visible);
            submitted += frame.SubmittedPages;
            selectionMs += frame.SelectionMs;
        }
        if (frame.Draws.Length == 0)
            throw new InvalidOperationException("G07 完整已提交覆盖未能恢复至 GPU 页池。");
        long selected = Stopwatch.GetTimestamp();
        var labels = G07Labels(source, cpu, mvp, width, height, interval, scale);
        style = style with { LabelGaps = labels.Select(label => LabelGap(label, scale)).ToArray() };
        long labeled = Stopwatch.GetTimestamp();
        string profile = probe.DrawScene(environment, width, height,
            frame.Draws.Select(draw => draw with { Style = style }).ToArray());
        long drawnAt = Stopwatch.GetTimestamp();
        profile = $"CPU选片={selectionMs:F3}ms 选片上传={Stopwatch.GetElapsedTime(start, selected).TotalMilliseconds:F1}ms " +
            $"标高={Stopwatch.GetElapsedTime(selected, labeled).TotalMilliseconds:F1}ms " +
            $"绘制等待={Stopwatch.GetElapsedTime(labeled, drawnAt).TotalMilliseconds:F1}ms " +
            $"标高数={labels.Length} {profile}";
        var leaves = frame.Coverage.Leaves;
        string levels = $"{leaves.Min(t => t.Level)}~{leaves.Max(t => t.Level)}";
        int drawn = frame.Draws.Count(d => d.ClipRects is null || d.ClipRects.Count > 0);
        return new(profile, submitted, levels, drawn, pool.ResidentCount, pool.PendingCount,
            pool.UploadedPages, pool.HeightBytes, Stopwatch.GetElapsedTime(start).TotalMilliseconds,
            frame.PendingCoverage is not null, frame.CapacityBlocked, labels,
            Stopwatch.GetElapsedTime(selected, labeled).TotalMilliseconds);
    }
}
