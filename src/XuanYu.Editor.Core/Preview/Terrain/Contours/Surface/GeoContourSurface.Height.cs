namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoContourSurface
{
    public float? HeightAt(float x, float y, bool center = false)
    {
        float? height = LookupHeight(x, y);
        CountLookup(center, x, y, height);
        return height;
    }

    private float? LookupHeight(float x, float y)
    {
        bool outside = x < _minX || y < _minY ||
            x > _minX + _spanX || y > _minY + _spanY;
        List<int>? candidates = outside ? null :
            _buckets[BinY(y) * _side + BinX(x)];
        ReadOnlySpan<float> data = _mesh.Vertices.Span;
        float? highest = null;
        if (candidates is not null)
            foreach (int i in candidates)
            {
                double ax = data[i], ay = data[i + 1];
                double bx = data[i + 7], by = data[i + 8];
                double cx = data[i + 14], cy = data[i + 15];
                double det = (by - cy) * (ax - cx) + (cx - bx) * (ay - cy);
                double a = ((by - cy) * (x - cx) + (cx - bx) * (y - cy)) / det;
                double b = ((cy - ay) * (x - cx) + (ax - cx) * (y - cy)) / det;
                double c = 1d - a - b;
                if (a < -1e-5d || b < -1e-5d || c < -1e-5d) continue;
                float z = (float)(a * data[i + 2] + b * data[i + 9] +
                    c * data[i + 16]);
                highest = highest is { } old ? MathF.Max(old, z) : z;
            }
        if (highest is not null) return highest;
        float? recovered = RecoverNearEdge(x, y);
        if (recovered is not null) RecoveryCount++;
        else if (outside) BoundsMissCount++;
        else if (candidates is null) BucketMissCount++;
        else TriangleMissCount++;
        return recovered;
    }
}