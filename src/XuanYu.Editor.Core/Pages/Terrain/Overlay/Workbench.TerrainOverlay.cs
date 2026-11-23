using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private GeoOverlayPreferences _geoOverlay = GeoOverlayPreferencesStore.Load();
    public bool GeoHudVisible => _geoOverlay.HudVisible;
    public bool GeoMiniMapVisible => _geoOverlay.MiniMapVisible;
    public int GeoMiniMapSize => _geoOverlay.MiniMapSize;

    public GeoTerrainMiniMapData? TerrainMiniMap() =>
        CurrentId == PageId.Terrain ? _geoPreview?.MiniMap() : null;

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