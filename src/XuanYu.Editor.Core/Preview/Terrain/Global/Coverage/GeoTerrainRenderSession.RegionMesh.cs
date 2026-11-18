using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private RenderMesh? BuildRectangularMesh(GeoTileId id,
        GeoTerrainEdgeNeighbors neighbors, bool native)
    {
        return native
            ? GeoTerrainNativeMeshBuilder.Build(_nativeSourceId, id, _anchor, _source, _coverage, neighbors)
            : GeoTerrainMeshBuilder.Build(GeoHeightTileBuilder.Build(_source, id, 16),
                _anchor, _source, _coverage, neighbors);
    }
}