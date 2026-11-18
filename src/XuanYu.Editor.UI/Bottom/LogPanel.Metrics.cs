using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class LogPanel
{
    private void RefreshLive()
    {
        if (_activeTab == "性能") RefreshPerformance();
        if (_activeTab == "调试") RefreshDebug();
    }

    private void RefreshPerformance()
    {
        TerrainStats? value = _wb.TerrainStats;
        if (value is null)
        {
            _metrics.Text = "当前没有地理地形会话；性能统计将在导入地图后显示。";
            return;
        }
        TerrainStats s = value.Value;
        var m = s.Mesh;
        var raw = s.Source;
        _metrics.Text =
            $"【可见瓦片】绘制 {s.DrawCount} / 目标 {s.SelectedCount} / 待细化 {s.PendingCount}\n\n" +
            $"【基础网格】{m.BaseCount}（本帧活跃 {m.BaseActiveCount}）\n" +
            $"【共边网格】{m.StitchedCount}（活跃 {m.StitchedActiveCount}）\n" +
            $"【Footprint】{m.FootprintCount}  【Native】{m.NativeCount}  【选择】{m.SelectionCount}\n" +
            $"【累计网格回收】{m.EvictedCount}\n\n" +
            $"【原始高程】{raw.ResidentCount}/{raw.Capacity} 片，" +
            $"{raw.ResidentBytes / 1048576d:F1} MiB\n" +
            $"【HGT 解码】{raw.LoadCount} 次  【淘汰】{raw.EvictedCount} 次\n" +
            $"【后台完成】{s.BackgroundLoads} 次\n" +
            $"【Middle 高程】{s.MiddleCount}/{s.MiddleCapacity} 片\n\n" +
            "帧耗时 / GPU 显存：尚未接入";
    }

    private void RefreshDebug()
    {
        if (_probe.IsChecked != true)
        {
            _debugText.Text = "探针关闭。启用后只读取当前会话的真实状态。";
            return;
        }
        var camera = _wb.GeoTerrainCamera;
        string position = camera is null ? "无地理相机" :
            $"目标纬经度 {camera.Target.LatitudeDegrees:F6}, " +
            $"{camera.Target.LongitudeDegrees:F6}\n相机距离 {camera.DistanceMeters:F1} m";
        _debugText.Text = $"页面：{_wb.Current.Title}\n" +
            $"GPU 状态：{_wb.ViewportNote}\n" + position +
            $"\n地理地形：{(_wb.IsGeoTerrainPreview ? "已绑定" : "未绑定")}";
    }
}