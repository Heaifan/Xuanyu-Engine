using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoTileMonitorTile(
    GeoTileId Id, GeoTileBounds Bounds, bool Targeted,
    bool Drawn, bool Cached, GeoTileId? Fallback)
{
    public string MeshAssetId { get; init; } = "";
    public string SourceHgt { get; init; } = "";
    public string ElevationSource { get; init; } = "";
    public double GeometryNorthMeters { get; init; }
    public double GeometryEastMeters { get; init; }

    public string Status => Drawn ? "当前绘制" : Targeted
        ? Fallback is null ? "待细化（无绘制父片）" : "待细化（父片兜底）"
        : Cached ? "仅基础缓存记录" : "不在缓存";
}
