using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private GeoTerrainRenderSession? _geoPreview;
    public bool IsGeoTerrainPreview => _geoPreview is not null;
    public GlobalCamera? GeoTerrainCamera => _geoPreview?.Camera;

    private void BindImportedTerrain(NasademTile? tile)
    {
        _geoPreview = tile is null ? null : new GeoTerrainRenderSession(tile);
        if (_geoPreview is not null)
        {
            _geoPreview.Notice += Log.Write;
            Log.Write($"G-03 地形正式视口已接入：{tile!.TileId}");
        }
    }

    public IReadOnlyList<RenderDraw> GeoTerrainDraws(uint width, uint height) =>
        _geoPreview!.Draws(width, height);

    public void NavigateGeoTerrain(float deltaX, float deltaY,
        float wheel, bool pan, uint height) =>
        _geoPreview!.Navigate(deltaX, deltaY, wheel, pan, height);

    public void ZoomGeoTerrainAt(float wheel, float x, float y,
        uint width, uint height) =>
        _geoPreview!.ZoomAtCursor(wheel, new Vector2(x, y), width, height);

    public void ZoomTerrainAt(float wheel, float x, float y,
        uint width, uint height) =>
        (_terrainPreview ??= CreateTerrainPreview()).ZoomAtCursor(
            wheel, new Vector2(x, y), width, height);

    public void SnapGeoTerrainAxis(Vector3 axis, uint width, uint height) =>
        _geoPreview!.SnapAxis(axis, width, height);

    public void ResetGeoTerrainCamera()
    {
        _geoPreview!.ResetCamera();
        Log.Write("G-03：已返回当前 NASADEM 地图中心");
    }
}