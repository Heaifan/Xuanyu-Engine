using System.Diagnostics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly bool _r05Hillshade = Environment.GetEnvironmentVariable("XYE_G06_DEM_HILLSHADE") == "1";
    private readonly Dictionary<RenderMesh, GeoTerrainDemPartition.Part[]> _demPartitions = [];

    private RenderDraw[] BindResidentDem(RenderDraw[] draws)
    {
        if (Camera.DistanceMeters > 20_000d)
        {
            _demPartitions.Clear();
            return draws;
        }
        long start = _r02PerfEnabled ? Stopwatch.GetTimestamp() : 0;
        Dictionary<RenderMesh, GeoTileId> owners = VisibleDemOwners();
        List<RenderDraw>? output = _r05Hillshade ? new(draws.Length) : null;
        if (output is not null) TrimDemPartitions(draws);
        var resident = new Dictionary<(int South, int West), RenderDemTile?>();
        int built = 0, crossing = 0, bound = 0;
        for (int i = 0; i < draws.Length; i++)
        {
            RenderDraw draw = draws[i];
            if (owners.TryGetValue(draw.Mesh, out GeoTileId id))
            {
                GeoTileBounds b = GeographicTileScheme.Bounds(id);
                int south = (int)Math.Floor(b.SouthLatitudeDegrees);
                int north = (int)Math.Ceiling(b.NorthLatitudeDegrees) - 1;
                int west = (int)Math.Floor(b.WestLongitudeDegrees);
                int east = (int)Math.Ceiling(b.EastLongitudeDegrees) - 1;
                if (south == north && west == east)
                    draw = draw with { ResidentDem = ReadResidentDem(resident, south, west) };
                else if (output is not null && id.Level >= 10 && !draw.Mesh.TerrainShadedColors.IsEmpty)
                {
                    crossing++;
                    if (!_demPartitions.TryGetValue(draw.Mesh, out var pieces))
                    {
                        pieces = GeoTerrainDemPartition.Split(draw.Mesh, _anchor, south, west, north, east);
                        _demPartitions.Add(draw.Mesh, pieces);
                        built++;
                    }
                    foreach (var part in pieces)
                    {
                        RenderDemTile? dem = ReadResidentDem(resident, part.South, part.West);
                        if (dem.HasValue) bound++;
                        output.Add(draw with { Mesh = part.Mesh, ResidentDem = dem, LegacyContourOnly = !dem.HasValue });
                    }
                    continue;
                }
            }
            if (draw.ResidentDem.HasValue) bound++;
            draws[i] = draw;
            output?.Add(draw);
        }
        RenderDraw[] result = output?.ToArray() ?? draws;
        if (_g06DiagnosticEnabled && _monitorFrame % 120 == 0)
            Notice?.Invoke($"[G06/DEM通道] 已附加={bound}/{result.Length}（CPU 数据）");
        if (_r05Hillshade)
            UpdateVisibleDemResidency(resident, crossing, built, bound, result.Length, start);
        return result;
    }

    private void TrimDemPartitions(RenderDraw[] draws)
    {
        var active = draws.Select(d => d.Mesh).ToHashSet();
        foreach (RenderMesh mesh in _demPartitions.Keys.ToArray())
            if (!active.Contains(mesh)) _demPartitions.Remove(mesh);
    }

    private Dictionary<RenderMesh, GeoTileId> VisibleDemOwners()
    {
        var owners = new Dictionary<RenderMesh, GeoTileId>();
        foreach (var (id, mesh) in _visibleMeshes)
        {
            owners[mesh] = id;
            if (!_selectionMeshes.TryGetValue(mesh, out var parts)) continue;
            if (parts.Inner is not null) owners[parts.Inner] = id;
            if (parts.Outer is not null) owners[parts.Outer] = id;
        }
        return owners;
    }
}