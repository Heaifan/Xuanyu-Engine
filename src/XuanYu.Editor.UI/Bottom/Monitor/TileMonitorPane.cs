using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed class TileMonitorPane : Grid
{
    private readonly TileMonitorCanvas _map = new() { Width = 600, Height = 600 };
    private readonly TextBlock _summary = new()
        { FontSize = 10, Foreground = EditorTheme.TextMain };
    private readonly TextBlock _detail = new()
        { FontSize = 10, TextWrapping = TextWrapping.Wrap,
          Foreground = EditorTheme.TextSub };
    private GeoTileId? _selected;

    public TileMonitorPane()
    {
        RowDefinitions = new RowDefinitions("38,*,44");
        Margin = new Thickness(8, 4);
        _map.Selected += tile => { _selected = tile.Id; ShowDetail(_map.Snapshot); };
        var square = new Viewbox
        {
            Child = _map, Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetRow(square, 1);
        Grid.SetRow(_detail, 2);
        Children.Add(_summary);
        Children.Add(square);
        Children.Add(_detail);
    }

    public void Update(GeoTileMonitorSnapshot? snapshot)
    {
        _map.SetSnapshot(snapshot, _selected);
        _summary.Text = snapshot is null ? "瓦片监视器 · 未载入地理地形" :
            $"帧 {snapshot.Frame} | 目标 {snapshot.TargetCount} | 实绘 {snapshot.VisibleCount} | " +
            $"待细化 {snapshot.PendingCount} | 北 ↑\n" +
            $"相机 {snapshot.Eye.LatitudeDegrees:F2},{snapshot.Eye.LongitudeDegrees:F2} → " +
            $"目标 {snapshot.Target.LatitudeDegrees:F2},{snapshot.Target.LongitudeDegrees:F2} " +
            $"· {snapshot.CameraDistanceMeters / 1000d:F0} km " +
            $"· 预取 {(snapshot.StreamingEnabled ? snapshot.RawPrefetchPending ? "进行中" : "空闲" : "未启用")}";
        ShowDetail(snapshot);
    }

    private void ShowDetail(GeoTileMonitorSnapshot? snapshot)
    {
        if (snapshot is null)
        {
            _detail.Text = "导入 NASADEM 后显示俯视瓦片图。";
            return;
        }
        GeoTileMonitorTile? selected = snapshot.Tiles
            .Where(tile => tile.Id == _selected)
            .Select(tile => (GeoTileMonitorTile?)tile).FirstOrDefault();
        if (selected is null)
        {
            _detail.Text = _selected is null
                ? "点击瓦片查看 ID、LOD、地理范围及缓存状态。蓝色=绘制，橙框=待细化，灰色=缓存。"
                : $"{_selected} 已不在当前目标/绘制/基础缓存集合中。";
            return;
        }
        GeoTileMonitorTile t = selected.Value;
        GeoTileBounds b = t.Bounds;
        _detail.Text = $"{t.Id}  LOD {t.Id.Level}  {t.Status} " +
            $"父片：{t.Fallback?.ToString() ?? "—"}\n" +
            $"纬 {b.SouthLatitudeDegrees:F5}～{b.NorthLatitudeDegrees:F5}° " +
            $"经 {b.WestLongitudeDegrees:F5}～{b.EastLongitudeDegrees:F5}° " +
            $"基础缓存：{(t.Cached ? "是" : "否")}";
    }
}