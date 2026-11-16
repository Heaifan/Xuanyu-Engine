using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private GeoTerrainRenderSession? _geoPreview;
    public bool IsGeoTerrainPreview => _geoPreview is not null;

    public void ToggleGeoTerrainPreview()
    {
        if (_geoPreview is not null)
        {
            _geoPreview = null;
            Log.Write("G-02F：已切回 E4 原地形预览");
        }
        else if (TerrainSourceTile is not NasademTile tile)
        {
            Log.Write("G-02F：请先导入 NASADEM HGT/ZIP，暂不支持直接从 XYMAP 启用");
            return;
        }
        else
        {
            _geoPreview = new GeoTerrainRenderSession(tile);
            _geoPreview.Notice += Log.Write;
            Log.Write($"G-02F：已启用真实米制瓦片预览：{tile.TileId}；滚轮缩放，MMB 旋转，Shift+MMB 平移");
        }
        Changed?.Invoke();
    }

    public IReadOnlyList<RenderDraw> GeoTerrainDraws(uint width, uint height) =>
        _geoPreview!.Draws(width, height);

    public void NavigateGeoTerrain(float deltaX, float deltaY,
        float wheel, bool pan, uint height) =>
        _geoPreview!.Navigate(deltaX, deltaY, wheel, pan, height);

    public void ResetGeoTerrainCamera()
    {
        _geoPreview!.ResetCamera();
        Log.Write("G-02F：已返回当前 NASADEM 的地图中心");
    }
}