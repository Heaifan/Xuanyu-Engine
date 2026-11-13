using System.IO;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI.Right;

internal sealed partial class RightTerrainInspector
{
    public void Refresh()
    {
        _rows.Children.Clear();
        MapDocument map = _wb.TerrainMap;
        TerrainHeightField field = map.HeightField;
        NasademTile? tile = _wb.TerrainSourceTile;

        AddGroup("地图信息", _showMapInfo, () => _showMapInfo = !_showMapInfo);
        if (_showMapInfo)
        {
            AddPair("名称", map.Name);
            AddPair("文件", _wb.TerrainMapPath is { } path
                ? Path.GetFileName(path) : "未保存");
        }

        AddGroup("地形高度场", _showHeightField,
            () => _showHeightField = !_showHeightField);
        if (_showHeightField)
        {
            AddPair("分辨率", $"{field.SamplesX} × {field.SamplesY}");
            AddPair("采样间距", $"{field.SpacingMeters:0.##} m");
            AddPair("尺寸", $"{map.WidthMeters:0} × {map.LengthMeters:0} m");
        }

        AddGroup("NASADEM 数据源", _showSource,
            () => _showSource = !_showSource);
        if (!_showSource) return;

        if (tile is null)
        {
            AddPair("类型", _wb.TerrainSourcePath is null
                ? "内置程序地形" : "XYE 地图文档");
            return;
        }

        var geo = new NasademGeoReference(tile);
        AddPair("瓦片", tile.TileId);
        AddPair("原始栅格", $"{NasademTile.Side} × {NasademTile.Side}");
        AddPair("最小高程", $"{tile.MinElevation} m");
        AddPair("最大高程", $"{tile.MaxElevation} m");
        AddPair("NoData", $"{tile.NoDataCount}");
        AddPair("纬度起点", $"{tile.SouthLatitude}°");
        AddPair("经度起点", $"{tile.WestLongitude}°");
        AddPair("地理范围",
            $"{geo.WidthMeters / 1000:0.0} × {geo.LengthMeters / 1000:0.0} km");
    }
}