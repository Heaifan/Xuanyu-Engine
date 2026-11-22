using System.Diagnostics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly Dictionary<GeoTileId, (RenderMesh Base, RenderMesh Refined)> _rawGeometry = [];
    public bool RawGeometryEnabled { get; set; } =
        Environment.GetEnvironmentVariable("XYE_G06_RAW_GEOMETRY") == "1";
    public int RawGeometryCount { get; private set; }

    private void UpdateRawGeometry()
    {
        RawGeometryCount = 0;
        if (!RawGeometryEnabled || !_source.HasOverview || _footprint is null || Camera.DistanceMeters > 20_000d)
        {
            _rawGeometry.Clear();
            return;
        }
        long start = _r02PerfEnabled ? Stopwatch.GetTimestamp() : 0;
        foreach (GeoTileId id in _rawGeometry.Keys.ToArray())
            if (!_visibleMeshes.ContainsKey(id)) _rawGeometry.Remove(id);
        int built = 0, waiting = 0, deferred = 0;
        var targets = _visibleMeshes.Keys.Where(id => id.Level == 13)
            .OrderBy(DistanceFromTarget).ToArray();
        foreach (GeoTileId id in targets)
        {
            RenderMesh mesh = _visibleMeshes[id];
            if (!mesh.AssetId.StartsWith("terrain/g02/", StringComparison.Ordinal) ||
                mesh.TerrainGeographicCoordinates.IsEmpty) continue;
            if (_rawGeometry.TryGetValue(id, out var cached) && ReferenceEquals(cached.Base, mesh))
            {
                _visibleMeshes[id] = cached.Refined;
                RawGeometryCount++;
                continue;
            }
            _rawGeometry.Remove(id);
            if (built == 4) { deferred++; continue; }
            if (ResidentGeometrySource(id) is not { } source) { waiting++; continue; }
            RenderMesh refined = GeoTerrainRawGeometry.Refine(mesh, source);
            _rawGeometry[id] = (mesh, refined);
            _visibleMeshes[id] = refined;
            built++;
            RawGeometryCount++;
        }
        if (_r02PerfEnabled && (built > 0 || _monitorFrame % 120 == 0))
            Notice?.Invoke($"[G06/R04B-GUI] 帧={_monitorFrame} 改良L13={RawGeometryCount} " +
                $"新建={built} 等驻留={waiting} 待预算={deferred} " +
                $"几何准备={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1}ms");
    }

    private NasademElevationSource? ResidentGeometrySource(GeoTileId id)
    {
        var footprint = _footprint!;
        GeoTileBounds b = footprint.Clip(id);
        var tiles = new List<NasademTile>(4);
        for (int south = (int)Math.Floor(b.SouthLatitudeDegrees); south < b.NorthLatitudeDegrees; south++)
            for (int west = (int)Math.Floor(b.WestLongitudeDegrees); west < b.EastLongitudeDegrees; west++)
            {
                if (!footprint.Covers(south + 0.5d, west + 0.5d)) continue;
                if (_source.TryGetResidentTile(south, west) is not { } tile) return null;
                tiles.Add(tile);
            }
        // 独立只读内存源，采样及法线查询不能触发原始 HGT 的同步解码。
        return tiles.Count == 0 ? null : new NasademElevationSource(tiles.ToArray());
    }
}