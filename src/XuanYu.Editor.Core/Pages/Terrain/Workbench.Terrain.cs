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
    public string TerrainNodeText => TerrainMultiSourceLabel ??
        (TerrainSourcePath is { } path ? Path.GetFileName(path) : TerrainMap.Name);

    public PreviewScene TerrainScene =>
        (_terrainPreview ??= CreateTerrainPreview()).Scene;
    public MapDocument TerrainMap =>
        (_terrainPreview ??= CreateTerrainPreview()).Map;

    public Vector3? PickTerrainPoint(Vector2 pixel, uint width, uint height)
    {
        if (CurrentId != PageId.Terrain || IsGeoTerrainPreview) return null;
        var session = _terrainPreview ??= CreateTerrainPreview();
        if (!session.TryPick(pixel, width, height, out var hit)) return null;
        return session.IsCentered
            ? (hit.WorldPosition - new Vector3(session.OriginMeters, 0f))
                * session.DisplayScale
            : hit.WorldPosition;
    }

    public void ImportTerrain(string path)
    {
        long started = System.Diagnostics.Stopwatch.GetTimestamp();
        NasademTile tile = NasademHgtFile.Load(path);
        TerrainWaterMask? water = NasademWaterFile.Load(path, tile);
        MapDocument map = NasademPreviewBuilder.Create(tile, water);
        var geo = new GeoTerrainRenderSession(tile);
        geo.LoadWater(tile, water);
        SetTerrain(map, tile, path, null, geo);
        TrackG07Import(geo, started);
        Log.Write($"G-03 NASADEM：{tile.TileId}，原始数据 {map.HeightField.SamplesX}×{map.HeightField.SamplesY}");
        Log.Write($"[G06/SWB] {geo.WaterStatus}");
    }

    public void OpenTerrainMap(string path)
    {
        MapDocument map = MapFile.Load(path);
        SetTerrain(map, null, path, path);
        Log.Write($"XYMAP 地图已打开：{map.Name}；原有 E4 渲染兼容路径");
    }

    public void SaveTerrainMap(string path)
    {
        if (TerrainMultiSourceLabel is not null)
            throw new InvalidOperationException("多 HGT 为多数据源预览，暂不支持保存为单张 XYMAP。");
        MapFile.Save(TerrainMap, path);
        TerrainMapPath = path;
        Log.Write($"地图已保存：{path}；分辨率 {TerrainMap.HeightField.SamplesX}×{TerrainMap.HeightField.SamplesY}");
        Changed?.Invoke();
    }

    public void SelectTerrain(bool selected)
    {
        if (TerrainSelected == selected && SelectedTerrainDem is null) return;
        ClearTerrainDemSelection();
        TerrainSelected = selected;
        TerrainScene.Select(null);
        Changed?.Invoke();
    }

    private void SetTerrain(MapDocument map, NasademTile? tile,
        string sourcePath, string? mapPath, GeoTerrainRenderSession? geo = null,
        IReadOnlyList<TerrainDemEntry>? entries = null)
    {
        _terrainPreview = new TerrainPreviewSession(map);
        BindTerrainScene(_terrainPreview.Scene);
        TerrainSourceTile = tile;
        TerrainSourcePath = sourcePath;
        TerrainMapPath = mapPath;
        TerrainMode = TerrainWorkspaceMode.Object;
        TerrainSelected = true;
        TerrainDemEntries = entries ?? [];
        SelectedTerrainDem = null;
        SelectedTerrainDemSummary = null;
        BindImportedTerrain(tile, geo);
        if (CurrentId != PageId.Terrain) SwitchTo(PageId.Terrain);
        Changed?.Invoke();
    }

}