using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly Dictionary<GeoTileId, RenderMesh> _visibleMeshes = [];
    private bool _firstRefinementLogged;
    public IReadOnlyCollection<GeoTileId> VisibleTileIds => _visibleMeshes.Keys;

    private void UpdateVisibleCover(IReadOnlyList<GeoTileId> targets)
    {
        _visibleMeshes.Clear();
        int budget = 4;
        if (_meshes[_coverRoot] is { } root)
            Cover(_coverRoot, root, targets, ref budget);
    }

    private void Cover(GeoTileId id, RenderMesh mesh,
        IReadOnlyList<GeoTileId> targets, ref int budget)
    {
        if (targets.Any(target => target.Level > id.Level && Contains(id, target)))
        {
            GeoTileId[] children = GeographicTileScheme.Children(id)
                .Where(InCoverage).ToArray();
            var prepared = new RenderMesh?[children.Length];
            for (int i = 0; i < children.Length; i++)
                prepared[i] = MeshFor(children[i], ref budget);

            // 每次替换必须完整覆盖父片在 HGT 内的全部区域。
            if (children.Length > 0 && prepared.All(item => item is not null))
            {
                for (int i = 0; i < children.Length; i++)
                    Cover(children[i], prepared[i]!, targets, ref budget);
                if (!_firstRefinementLogged)
                {
                    _firstRefinementLogged = true;
                    Notice?.Invoke($"G-04-03B-R3：父片 {id} 完整替换为 {children.Length} 片子网格");
                }
                return;
            }
        }
        _visibleMeshes.Add(id, mesh);
    }

    private RenderMesh? MeshFor(GeoTileId id, ref int budget)
    {
        if (_meshes.TryGetValue(id, out RenderMesh? mesh)) return mesh;
        if (budget == 0) return null;
        budget--;
        mesh = BuildCoarseMesh(id);
        _meshes.Add(id, mesh);
        return mesh;
    }

    private RenderMesh? BuildCoarseMesh(GeoTileId id)
    {
        GeoHeightTile tile = GeoHeightTileBuilder.Build(_source, id, 16);
        return GeoTerrainMeshBuilder.Build(tile, _anchor, _source, _coverage);
    }

    private bool InCoverage(GeoTileId id)
    {
        GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
        return bounds.SouthLatitudeDegrees < _coverage.NorthLatitudeDegrees &&
            bounds.NorthLatitudeDegrees > _coverage.SouthLatitudeDegrees &&
            bounds.WestLongitudeDegrees < _coverage.EastLongitudeDegrees &&
            bounds.EastLongitudeDegrees > _coverage.WestLongitudeDegrees;
    }

    private static bool Contains(GeoTileId outer, GeoTileId inner)
    {
        if (outer.Level > inner.Level) return false;
        int shift = inner.Level - outer.Level;
        return inner.X >> shift == outer.X && inner.Y >> shift == outer.Y;
    }
}