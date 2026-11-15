using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal sealed partial class TerrainTileGeometry
{
    private readonly TerrainHeightField _field;
    private readonly int _startX, _startY, _cellsX, _cellsY, _step;
    private readonly int[] _lookup;
    private readonly List<Vector3> _positions = [];
    private readonly List<Vector3> _normals = [];
    private readonly List<int> _indices = [];

    private TerrainTileGeometry(TerrainHeightField field,
        int x, int y, int width, int height, int step)
    {
        _field = field;
        _startX = x;
        _startY = y;
        _cellsX = width;
        _cellsY = height;
        _step = step;
        _lookup = new int[(width + 1) * (height + 1)];
        Array.Fill(_lookup, -1);
    }

    public static TerrainMeshGeometry Create(TerrainHeightField field,
        int startX, int startY, int width, int height, int step)
    {
        var tile = new TerrainTileGeometry(field, startX, startY,
            width, height, step);
        tile.AddInterior();
        tile.AddEdges();
        return new TerrainMeshGeometry(tile._positions.ToArray(),
            tile._normals.ToArray(), tile._indices.ToArray());
    }

    private void AddInterior()
    {
        for (int y = _step; y < _cellsY - _step; y += _step)
            for (int x = _step; x < _cellsX - _step; x += _step)
            {
                Tri(x, y, x + _step, y, x + _step, y + _step);
                Tri(x, y, x + _step, y + _step, x, y + _step);
            }
    }

    private void Tri(int ax, int ay, int bx, int by, int cx, int cy)
    {
        _indices.Add(Vertex(ax, ay));
        _indices.Add(Vertex(bx, by));
        _indices.Add(Vertex(cx, cy));
    }

    private void Tri((int X, int Y) a, (int X, int Y) b, (int X, int Y) c) =>
        Tri(a.X, a.Y, b.X, b.Y, c.X, c.Y);

    private int Vertex(int x, int y)
    {
        int index = y * (_cellsX + 1) + x;
        if (_lookup[index] >= 0) return _lookup[index];
        int gx = _startX + x, gy = _startY + y;
        int left = Math.Max(0, gx - 5), right = Math.Min(_field.SamplesX - 1, gx + 5);
        int down = Math.Max(0, gy - 5), up = Math.Min(_field.SamplesY - 1, gy + 5);
        float dx = (_field.GetHeight(left, gy) - _field.GetHeight(right, gy)) /
            ((right - left) * _field.SpacingXMeters);
        float dy = (_field.GetHeight(gx, down) - _field.GetHeight(gx, up)) /
            ((up - down) * _field.SpacingYMeters);
        _lookup[index] = _positions.Count;
        _positions.Add(new Vector3(gx * _field.SpacingXMeters,
            gy * _field.SpacingYMeters, _field.GetHeight(gx, gy)));
        _normals.Add(Vector3.Normalize(new Vector3(dx, dy, 1f)));
        return _lookup[index];
    }
}