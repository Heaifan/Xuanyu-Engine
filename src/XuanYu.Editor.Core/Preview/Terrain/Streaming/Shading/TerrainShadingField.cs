using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed class TerrainShadingField
{
    private const int Interval = 20;
    private readonly Vector3[] _colors;
    private readonly int _columns, _maxX, _maxY;

    public TerrainShadingField(TerrainHeightField field,
        (float Min, float Max) heightRange)
    {
        _maxX = field.SamplesX - 1;
        _maxY = field.SamplesY - 1;
        _columns = (_maxX + Interval - 1) / Interval + 1;
        int rows = (_maxY + Interval - 1) / Interval + 1;
        _colors = new Vector3[_columns * rows];
        for (int row = 0; row < rows; row++)
            for (int col = 0; col < _columns; col++)
                _colors[row * _columns + col] = ColorAt(field,
                    Math.Min(col * Interval, _maxX),
                    Math.Min(row * Interval, _maxY));
    }

    public Vector3 Sample(int x, int y)
    {
        int col = Math.Min(x / Interval, _columns - 2);
        int row = Math.Min(y / Interval, _colors.Length / _columns - 2);
        int nextX = Math.Min((col + 1) * Interval, _maxX);
        int nextY = Math.Min((row + 1) * Interval, _maxY);
        float tx = (x - col * Interval) / (float)(nextX - col * Interval);
        float ty = (y - row * Interval) / (float)(nextY - row * Interval);
        Vector3 a = _colors[row * _columns + col];
        Vector3 b = _colors[row * _columns + col + 1];
        Vector3 c = _colors[(row + 1) * _columns + col];
        Vector3 d = _colors[(row + 1) * _columns + col + 1];
        return tx >= ty
            ? a + (b - a) * tx + (d - b) * ty
            : a + (c - a) * ty + (d - c) * tx;
    }

    private static Vector3 ColorAt(TerrainHeightField field, int x, int y)
    {
        int l = Math.Max(0, x - 5), r = Math.Min(field.SamplesX - 1, x + 5);
        int d = Math.Max(0, y - 5), u = Math.Min(field.SamplesY - 1, y + 5);
        float dx = (field.GetHeight(l, y) - field.GetHeight(r, y)) /
            ((r - l) * field.SpacingXMeters);
        float dy = (field.GetHeight(x, d) - field.GetHeight(x, u)) /
            ((u - d) * field.SpacingYMeters);
        Vector3 normal = Vector3.Normalize(new Vector3(dx, dy, 1f));
        Vector3 point = field.GetWorldPosition(x, y);
        return GeoTerrainEdgeVertex.FromHeight(point, point.Z, normal).ShadedColor;
    }
}