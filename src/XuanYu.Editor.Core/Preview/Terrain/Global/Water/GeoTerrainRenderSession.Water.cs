using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private RenderWaterAtlas? _waterAtlas;
    public int LoadedWaterMaskCount => _waterAtlas?.LoadedCount ?? 0;
    public string WaterStatus { get; private set; } = "无 SWB，不猜测水域";

    public void LoadWater(NasademTileCatalog catalog, IReadOnlyList<NasademHgtInfo> sources)
    {
        _waterAtlas = GeoTerrainWaterAtlas.Build(_coverage, sources,
            info => catalog.LoadWaterMask(info.SouthLatitude, info.WestLongitude), out string status);
        WaterStatus = _waterAtlas is null ? status : $"已加载 {LoadedWaterMaskCount}/{SourceCount} 片（SWB）";
    }

    public void LoadWater(NasademTile tile, TerrainWaterMask? mask)
    {
        NasademHgtInfo[] sources = mask is null ? [] :
            [new(tile.TileId, tile.SouthLatitude, tile.WestLongitude)];
        _waterAtlas = GeoTerrainWaterAtlas.Build(_coverage, sources, _ => mask, out string status);
        WaterStatus = _waterAtlas is null ? status : "已加载 1/1 片（SWB）";
    }
}