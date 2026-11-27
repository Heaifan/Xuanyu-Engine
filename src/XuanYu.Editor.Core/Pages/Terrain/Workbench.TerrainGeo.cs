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
    public string? TerrainMultiSourceLabel => _geoPreview is { IsMultiSource: true } geo
        ? geo.SourceLabel : null;
    public int TerrainSourceCount => _geoPreview?.SourceCount ?? 0;
    public string TerrainWaterStatus => _geoPreview?.WaterStatus ?? "未导入";
    public bool TerrainContoursEnabled => _geoPreview?.ContoursEnabled ??
        (HasLocalTerrainContours && _localContoursEnabled);
    public int TerrainContourInterval => _geoPreview?.ContourStyle.IntervalMeters ??
        (_terrainPreview is { IsCentered: true } local ? GetLocalContourStyle(local).IntervalMeters : 0);

    public TerrainStats? TerrainStats => _geoPreview is { } geo ? new(
        geo.MeshCacheStats, geo.SourceResidency, geo.DrawCount,
        geo.SelectedCount, geo.PendingCount, geo.OverviewMiddleCount,
        geo.OverviewMiddleCapacity, geo.BackgroundLoadCount) : null;

    private void BindImportedTerrain(NasademTile? tile, GeoTerrainRenderSession? geo)
    {
        _geoPreview = geo ?? (tile is null ? null : new GeoTerrainRenderSession(tile));
        if (_geoPreview is not null)
        {
            _geoPreview.Notice += Log.Write;
            Log.Write($"G-03 地形正式视口已接入：{_geoPreview.SourceLabel}");
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
        uint width, uint height)
    {
        if (CurrentId == PageId.Terrain || ReferenceEquals(_terrainPreview?.Scene, Scene))
            (_terrainPreview ??= CreateTerrainPreview()).ZoomAtCursor(
                wheel, new Vector2(x, y), width, height);
        else Scene.Camera?.Zoom(wheel);
    }

    public void SnapGeoTerrainAxis(Vector3 axis, uint width, uint height) =>
        _geoPreview!.SnapAxis(axis, width, height);

    public void ResetGeoTerrainCamera()
    {
        _geoPreview!.ResetCamera();
        Log.Write("G-03：已返回当前 NASADEM 地图中心");
    }

    public void ToggleGeoTerrainNative()
    {
        _geoPreview!.ToggleNative();
        Log.Write($"G-04/AB：Native={(_geoPreview.NativeEnabled ? "ON" : "OFF")}");
    }

    public void SetTerrainContoursEnabled(bool enabled)
    {
        if (TerrainContoursEnabled == enabled) return;
        if (_geoPreview is { } geo)
        {
            geo.SetContoursEnabled(enabled);
            Log.Write($"G-06 等高线：{(enabled ? "开启" : "关闭")}");
            Changed?.Invoke();
        }
        else if (HasLocalTerrainContours)
        {
            _localContoursEnabled = enabled;
            Log.Write($"[E5/V02] 等高线：{(enabled ? "开启" : "关闭")}");
            Changed?.Invoke();
        }
    }
}