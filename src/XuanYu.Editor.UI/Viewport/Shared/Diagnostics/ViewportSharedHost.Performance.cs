using System.Diagnostics;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private long _sharedPerfLastStart, _sharedPerfFrames;

    private double BeginSharedFrameProfile(long started, bool enabled)
    {
        if (!enabled) return 0;
        double interval = _sharedPerfLastStart == 0 ? 0 :
            Stopwatch.GetElapsedTime(_sharedPerfLastStart, started).TotalMilliseconds;
        _sharedPerfLastStart = started;
        return interval;
    }

    private void ReportSharedFrameProfile(bool enabled, double interval,
        long started, long prepared, long prepareStart, long brushed,
        long drawn, long presented, long uiUpdated, string gpu)
    {
        if (!enabled || ++_sharedPerfFrames % 60 != 0) return;
        double total = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
        _wb.Log.Write($"[G06/R08-04A] 帧={_sharedPerfFrames} 间隔={interval:F1}ms " +
            $"总计={total:F1}ms UI帧准备={Stopwatch.GetElapsedTime(prepareStart, prepared).TotalMilliseconds:F1}ms " +
            $"地编工具={Stopwatch.GetElapsedTime(prepared, brushed).TotalMilliseconds:F1}ms " +
            $"Vulkan({gpu}) 共享纹理合成调用={Stopwatch.GetElapsedTime(drawn, presented).TotalMilliseconds:F1}ms " +
            $"UI叠加={Stopwatch.GetElapsedTime(presented, uiUpdated).TotalMilliseconds:F1}ms " +
            $"等高线={_wb.TerrainContoursEnabled} 标高数字={_wb.TerrainContourLabelsEnabled}");
    }
}
