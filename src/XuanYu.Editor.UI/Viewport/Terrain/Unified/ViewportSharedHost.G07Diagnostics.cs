namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private long _g07LastLogAt;

    private void ReportG07Frame(int sourceCount, G07FrameResult result, double environmentMs)
    {
        ++_g07Frames;
        bool profile = Environment.GetEnvironmentVariable("XYE_G06_PERF") == "1";
        long now = Environment.TickCount64;
        if (_g07Frames != 1 && (!profile || now - _g07LastLogAt < 1000)) return;
        _g07LastLogAt = now;
        _wb.Log.Write($"[G07/视口] 帧={_g07Frames} 路径=GPU HGT={sourceCount} " +
            $"LOD={result.Levels} 绘制={result.Drawn} 驻留={result.Resident} 待发布={result.Pending} " +
            $"上传={result.TotalPages} 高程显存={result.Bytes}B 本帧={result.Ms:F1}ms " +
            $"切换等待={result.Transition} 容量阻塞={result.Blocked} CPU像素读回=0 " +
            $"指标={_g07Cpu!.Diagnostic ?? "正常"} " +
            (profile ? $"环境={environmentMs:F1}ms " : "") + result.Profile);
    }
}