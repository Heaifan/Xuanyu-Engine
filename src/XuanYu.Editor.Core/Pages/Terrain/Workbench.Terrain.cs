using System.IO;
using System.Numerics;
using XuanYu.Editor.Preview;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private TerrainPreviewSession? _terrainPreview;
    public NasademTile? TerrainSourceTile { get; private set; }
    public string? TerrainSourcePath { get; private set; }
    public string? TerrainMapPath { get; private set; }
    public bool TerrainSelected { get; private set; } = true;
    public string TerrainNodeText => TerrainSourcePath is { } path
        ? Path.GetFileName(path) : "地形高度场";

    public PreviewScene TerrainScene =>
        (_terrainPreview ??= CreateTerrainPreview()).Scene;
    public MapDocument TerrainMap =>
        (_terrainPreview ??= CreateTerrainPreview()).Map;

    public Vector3? PickTerrainPoint(Vector2 pixel, uint width, uint height)
    {
        if (CurrentId != PageId.Terrain || IsGeoTerrainPreview) return null;
        var session = _terrainPreview ??= CreateTerrainPreview();
        return session.TryPick(pixel, width, height, out var hit)
            ? hit.WorldPosition : null;
    }

    public void ImportTerrain(string path)
    {
        NasademTile tile = NasademHgtFile.Load(path);
        TerrainWaterMask? water = NasademWaterFile.Load(path, tile);
        MapDocument map = NasademPreviewBuilder.Create(tile, water);
        SetTerrain(map, tile, path, null);
        Log.Write($"E4 NASADEM：{tile.TileId}；{map.HeightField.SamplesX}×{map.HeightField.SamplesY}");
        Log.Write(water is null
            ? "E4 水体：未发现对应 SWB，保持原地表色，不猜测海洋。"
            : "E4 水体：SWB 已导入，蓝色水面及岸线着色已启用。");
    }

    public void OpenTerrainMap(string path)
    {
        MapDocument map = MapFile.Load(path);
        SetTerrain(map, null, path, path);
        Log.Write($"E4 地图已打开：{map.Name}；SWB={(map.WaterMask is null ? "无" : "有")}");
    }

    public void SaveTerrainMap(string path)
    {
        MapFile.Save(TerrainMap, path);
        TerrainMapPath = path;
        Log.Write($"E4 地图已保存：{path}；分辨率 {TerrainMap.HeightField.SamplesX}×{TerrainMap.HeightField.SamplesY}");
        Changed?.Invoke();
    }

    public void SelectTerrain(bool selected)
    {
        if (TerrainSelected == selected) return;
        TerrainSelected = selected;
        TerrainScene.Select(null);
        Changed?.Invoke();
    }

    private void SetTerrain(MapDocument map, NasademTile? tile,
        string sourcePath, string? mapPath)
    {
        _geoPreview = null;
        _terrainPreview = new TerrainPreviewSession(map);
        BindTerrainScene(_terrainPreview.Scene);
        TerrainSourceTile = tile;
        TerrainSourcePath = sourcePath;
        TerrainMapPath = mapPath;
        TerrainSelected = true;
        if (CurrentId != PageId.Terrain) SwitchTo(PageId.Terrain);
        Changed?.Invoke();
    }

    private TerrainPreviewSession CreateTerrainPreview()
    {
        var preview = new TerrainPreviewSession();
        BindTerrainScene(preview.Scene);
        Log.Write($"E4 地形预览已创建：{preview.Map.WidthMeters:0} × {preview.Map.LengthMeters:0} 米");
        return preview;
    }

    private void BindTerrainScene(PreviewScene scene) =>
        scene.Changed += () => Changed?.Invoke();
}