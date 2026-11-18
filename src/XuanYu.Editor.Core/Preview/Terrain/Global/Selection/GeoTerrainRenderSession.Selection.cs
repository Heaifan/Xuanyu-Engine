using System.Numerics;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private GeoTileBounds? _selectedDem;
    private readonly Dictionary<RenderMesh, (RenderMesh? Inner, RenderMesh? Outer)>
        _selectionMeshes = [];

    internal void SetSelectedDem(GeoTileBounds? bounds)
    {
        if (_selectedDem == bounds) return;
        _selectedDem = bounds;
        _selectionMeshes.Clear();
    }

    private RenderDraw[] CreateSelectionDraws(Matrix4x4 projection)
    {
        if (_selectedDem is not { } selected)
            return _visibleMeshes.Values.Select(mesh =>
                new RenderDraw(mesh, projection, true, false)).ToArray();
        var active = _visibleMeshes.Values.ToHashSet();
        foreach (RenderMesh mesh in _selectionMeshes.Keys.ToArray())
            if (!active.Contains(mesh)) _selectionMeshes.Remove(mesh);
        var draws = new List<RenderDraw>();
        foreach (var (id, mesh) in _visibleMeshes)
        {
            var (inner, outer) = SelectionMeshes(id, mesh, selected);
            if (outer is not null) draws.Add(new RenderDraw(outer, projection, true, false));
            if (inner is not null) draws.Add(new RenderDraw(inner, projection, true, true)
            { OutlineThroughOcclusion = true });
        }
        return draws.ToArray();
    }

    private (RenderMesh? Inner, RenderMesh? Outer) SelectionMeshes(
        GeoTileId id, RenderMesh mesh, GeoTileBounds selected)
    {
        GeoTileBounds area = GeographicTileScheme.Bounds(id);
        if (!Overlaps(area, selected)) return (null, mesh);
        bool inside = area.SouthLatitudeDegrees >= selected.SouthLatitudeDegrees &&
            area.NorthLatitudeDegrees <= selected.NorthLatitudeDegrees &&
            area.WestLongitudeDegrees >= selected.WestLongitudeDegrees &&
            area.EastLongitudeDegrees <= selected.EastLongitudeDegrees;
        if (inside) return (mesh, null);
        if (_selectionMeshes.TryGetValue(mesh, out var cached)) return cached;
        var parts = GeoTerrainSelectionMesh.Split(mesh, _anchor, selected);
        _selectionMeshes.Add(mesh, parts);
        return parts;
    }
}