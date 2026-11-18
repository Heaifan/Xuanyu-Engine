using XuanYu.World.Maps;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    private bool _showSelectedDem = true;

    private void RefreshMulti(string label)
    {
        AddGroup("地图信息", _showMapInfo, () => _showMapInfo = !_showMapInfo);
        if (_showMapInfo)
        {
            AddPair("模式", "多 HGT 预览");
            AddPair("文件", "暂不支持多 HGT 保存");
        }
        AddGroup("NASADEM 数据源", _showSource, () => _showSource = !_showSource);
        if (_showSource)
        {
            AddPair("瓦片数量", $"{_wb.TerrainSourceCount}");
            AddPair("数据集", label);
            AddPair("原始栅格", "每片 3601 × 3601");
            AddPair("采样间距", "1 角秒（原始栅格）");
            AddPair("水体着色", "待接入");
            if (_wb.TerrainGeoCoverage is { } coverage) AddBounds(coverage);
        }
        AddGroup("当前选中瓦片", _showSelectedDem, () => _showSelectedDem = !_showSelectedDem);
        if (!_showSelectedDem) return;
        if (_wb.SelectedTerrainDem is not { } entry)
        {
            AddPair("选择", "数据集整体");
            return;
        }
        var geo = new NasademGeoReference(entry.Info);
        AddPair("瓦片", entry.TileId);
        AddPair("文件", System.IO.Path.GetFileName(entry.Path));
        AddBounds(entry.Bounds);
        AddPair("原始栅格", $"{NasademTile.Side} × {NasademTile.Side}");
        AddPair("尺寸", $"{geo.WidthMeters / 1000:0.0} × {geo.LengthMeters / 1000:0.0} km");
        if (_wb.SelectedTerrainDemSummary is { } summary)
        {
            AddPair("最小高程", $"{summary.MinElevation} m");
            AddPair("最大高程", $"{summary.MaxElevation} m");
            AddPair("NoData", $"{summary.NoDataCount}");
        }
        AddPair("SWB", entry.HasWaterMask ? "已发现（未加载）" : "未发现");
    }

    private void AddBounds(GeoTileBounds bounds)
    {
        AddPair("纬度范围", $"{bounds.SouthLatitudeDegrees:0.####}° ～ {bounds.NorthLatitudeDegrees:0.####}°");
        AddPair("经度范围", $"{bounds.WestLongitudeDegrees:0.####}° ～ {bounds.EastLongitudeDegrees:0.####}°");
    }
}