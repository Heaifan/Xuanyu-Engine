using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoContourSurface
{
    private readonly RenderMesh _mesh;
    private readonly List<int>?[] _buckets;
    private readonly int _side;
    private readonly float _minX;
    private readonly float _minY;
    private readonly float _spanX;
    private readonly float _spanY;

    public float SpacingMeters { get; }

    public GeoContourSurface(RenderMesh mesh)
    {
        _mesh = mesh;
        ReadOnlySpan<float> data = mesh.Vertices.Span;
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;
        for (int i = 0; i < data.Length; i += 7)
        {
            minX = MathF.Min(minX, data[i]);
            maxX = MathF.Max(maxX, data[i]);
            minY = MathF.Min(minY, data[i + 1]);
            maxY = MathF.Max(maxY, data[i + 1]);
        }
        _minX = minX;
        _minY = minY;
        _spanX = maxX - minX;
        _spanY = maxY - minY;
        _side = Math.Clamp((int)MathF.Ceiling(
            MathF.Sqrt(mesh.TriangleCount / 2f)), 4, 128);
        _buckets = new List<int>?[_side * _side];
        SpacingMeters = MathF.Max(1f, MathF.Sqrt(
            _spanX * _spanY / MathF.Max(1f, mesh.TriangleCount / 2f)) * 0.5f);
        BuildIndex();
    }

    private int BinX(float x) => Math.Clamp(
        (int)((x - _minX) / _spanX * _side), 0, _side - 1);
    private int BinY(float y) => Math.Clamp(
        (int)((y - _minY) / _spanY * _side), 0, _side - 1);
}