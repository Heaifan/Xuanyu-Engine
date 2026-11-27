using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private GeoOverlayPreferences _geoOverlay = GeoOverlayPreferencesStore.Load();
    public bool TerrainHillshadeEnabled => _geoOverlay.HillshadeEnabled;
    public bool GeoHudVisible => _geoOverlay.HudVisible;
    public bool GeoMiniMapVisible => _geoOverlay.MiniMapVisible;
    public int GeoMiniMapSize => _geoOverlay.MiniMapSize;

    public GeoTerrainMiniMapData? TerrainMiniMap() => CurrentId != PageId.Terrain
        ? null : _geoPreview is { } geo ? geo.MiniMap() : LocalTerrainMiniMap();

    public void SetTerrainHillshadeEnabled(bool enabled)
    {
        if (TerrainHillshadeEnabled == enabled) return;
        _geoOverlay = _geoOverlay with { HillshadeEnabled = enabled };
        Log.Write($"[E5/V03] 地形晕渲：{(enabled ? "开启" : "关闭")}");
        SaveGeoOverlayPreferences();
    }

    public IReadOnlyList<RenderDraw> ApplyTerrainHillshade(IReadOnlyList<RenderDraw> draws)
    {
        if ((IsSimulating && CurrentId != PageId.Terrain) || TerrainHillshadeEnabled) return draws;
        return draws.Select(draw => draw.Mesh.TerrainShadedColors.IsEmpty ? draw :
            draw with { DisableTerrainHillshade = true }).ToArray();
    }

    public void SetGeoHudVisible(bool visible)
    {
        if (GeoHudVisible == visible) return;
        _geoOverlay = _geoOverlay with { HudVisible = visible };
        SaveGeoOverlayPreferences();
    }

    public void SetGeoMiniMapVisible(bool visible)
    {
        if (GeoMiniMapVisible == visible) return;
        _geoOverlay = _geoOverlay with { MiniMapVisible = visible };
        SaveGeoOverlayPreferences();
    }

    public void SetGeoMiniMapSize(int size, bool save = true)
    {
        int next = Math.Clamp(size, 120, 280);
        if (GeoMiniMapSize == next) return;
        _geoOverlay = _geoOverlay with { MiniMapSize = next };
        if (save) SaveGeoOverlayPreferences();
    }

    public void SaveGeoOverlayPreferences()
    {
        try { GeoOverlayPreferencesStore.Save(_geoOverlay); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            Log.Write($"[G06/UX02] 地理叠层设置保存失败：{error.Message}");
        }
        Changed?.Invoke();
    }
}