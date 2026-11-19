using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

public sealed partial class TileMonitorWindow
{
    private void Refresh()
    {
        GeoTileMonitorHistory? current = _wb.TileMonitorHistory;
        if (!ReferenceEquals(_history, current))
        {
            _history?.SetRecording(false);
            _history = current;
            _cursor = 0;
        }
        if (_history is { Count: > 0 } history)
        {
            _cursor = history.IsRecording ? history.Count - 1
                : Math.Min(_cursor, history.Count - 1);
            GeoTileMonitorFrame frame = history[_cursor];
            _pane.Update(frame.Snapshot);
            _stats.Text = FormatHistory(frame,
                _cursor > 0 ? history[_cursor - 1] : null);
        }
        else
        {
            _pane.Update(_wb.TileMonitor);
            _stats.Text = FormatLive();
        }
        SyncControls();
    }

    private static string FormatHistory(GeoTileMonitorFrame frame,
        GeoTileMonitorFrame? previous)
    {
        var s = frame.Snapshot;
        var m = frame.Mesh;
        var raw = frame.Source;
        int added = 0, removed = 0;
        if (previous is { } prior)
        {
            var oldIds = prior.Snapshot.Tiles.Where(t => t.Drawn)
                .Select(t => t.Id).ToHashSet();
            var newIds = s.Tiles.Where(t => t.Drawn)
                .Select(t => t.Id).ToHashSet();
            added = newIds.Except(oldIds).Count();
            removed = oldIds.Except(newIds).Count();
        }
        int fallback = s.Tiles.Count(t => t.Targeted && !t.Drawn && t.Fallback is not null);
        long evicted = m.EvictedCount - (previous?.Mesh.EvictedCount ?? m.EvictedCount);
        int nativeDelta = frame.NativeDrawCount - (previous?.NativeDrawCount ?? frame.NativeDrawCount);
        long hgtDelta = raw.LoadCount - (previous?.Source.LoadCount ?? raw.LoadCount);
        return $"记录 {frame.CapturedAt:HH:mm:ss.fff} · 帧 {s.Frame} · 新实绘 +{added} / 消失 -{removed}\n" +
            $"目标 {s.TargetCount} · 实绘 {s.VisibleCount} · 待细化 {s.PendingCount} · 父片兜底 {fallback} · Native {frame.NativeDrawCount}（变化 {nativeDelta:+#;-#;0}）\n" +
            $"网格 {m.BaseCount}（活跃 {m.BaseActiveCount}） · 共边 {m.StitchedCount} · " +
            $"回收累计 {m.EvictedCount}（较前帧 +{evicted}）\n" +
            $"HGT {raw.ResidentCount}/{raw.Capacity} · 解码 {raw.LoadCount}（新增 {hgtDelta}） · " +
            $"后台完成 {frame.BackgroundLoads} · Middle {frame.MiddleCount}/{frame.MiddleCapacity}";
    }

    private string FormatLive()
    {
        if (_wb.TerrainStats is not { } s)
            return "未导入地理地形；加载 NASADEM 后可录制真实渲染帧。";
        var m = s.Mesh;
        var raw = s.Source;
        return $"实时预览（未录制） · 绘制 {s.DrawCount} · 目标 {s.SelectedCount}\n" +
            $"网格 {m.BaseCount}（活跃 {m.BaseActiveCount}） · 回收累计 {m.EvictedCount}\n" +
            $"HGT {raw.ResidentCount}/{raw.Capacity} · 解码 {raw.LoadCount} · " +
            $"后台完成 {s.BackgroundLoads} · Middle {s.MiddleCount}/{s.MiddleCapacity}";
    }
}