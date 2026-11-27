using XuanYu.Core.Geodesy;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Core.Preview.Terrain.Unified;

public static class GeoUnifiedMeshAdapter
{
    public static RenderMesh Build(GeoUnifiedHeightPage page, EnuFrame anchor, int edgeMask)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(anchor);
        if ((uint)edgeMask > 15) throw new ArgumentOutOfRangeException(nameof(edgeMask));
        const int count = 33 * 33;
        var vertices = new float[count * 7];
        var valid = new bool[count];
        int step = 1 << page.Tile.Level;
        for (int y = 0; y < 33; y++)
            for (int x = 0; x < 33; x++)
            {
                int i = y * 33 + x;
                if (page.HeightAt(x, y) is not short height) continue;
                var p = GeoUnifiedPosition.At(anchor, page.Tile.West + x * step,
                    page.Tile.South + y * step, height);
                float t = Math.Clamp(height / 4000f, 0f, 1f);
                int v = i * 7;
                vertices[v] = (float)p.East; vertices[v + 1] = (float)p.North; vertices[v + 2] = (float)p.Up;
                vertices[v + 3] = 0.09f + 0.61f * t; vertices[v + 4] = 0.32f + 0.38f * t;
                vertices[v + 5] = 0.10f + 0.53f * t; vertices[v + 6] = 1f;
                valid[i] = true;
            }
        int[] topology = GeoUnifiedGridIndices.Create(edgeMask);
        var indices = new List<uint>(topology.Length);
        for (int i = 0; i < topology.Length; i += 3)
            if (valid[topology[i]] && valid[topology[i + 1]] && valid[topology[i + 2]])
            { indices.Add((uint)topology[i]); indices.Add((uint)topology[i + 1]); indices.Add((uint)topology[i + 2]); }
        if (indices.Count == 0) throw new InvalidOperationException("高程页没有有效地形三角形。");
        return RenderMesh.FromIndexedTerrain($"G07:{page.Tile}", vertices, indices.ToArray());
    }
}