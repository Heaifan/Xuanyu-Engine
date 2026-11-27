using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public sealed class TerrainNaturalRidgeStamp
{
    private readonly TerrainHeightField _field, _reference;
    private readonly Vector2 _origin;
    private readonly double _radius, _height, _apronExponent, _coreExponent;
    private readonly TerrainFractalNoise _noise, _crestNoise;
    private readonly TerrainDomainWarp _warp;

    public TerrainNaturalRidgeStamp(TerrainHeightField field, TerrainHeightField reference,
        TerrainBrushSettings brush, float height, int seed, Vector2 origin)
    {
        _field = field; _reference = reference; _origin = origin;
        _radius = brush.RadiusMeters; _height = height;
        _apronExponent = 1d + 0.8d * brush.Falloff;
        _coreExponent = 1d + 0.4d * brush.Falloff;
        double wavelength = _radius * 0.85d;
        double spacing = Math.Max(field.SpacingXMeters, field.SpacingYMeters);
        int octaves = (int)Math.Clamp(1d + Math.Floor(Math.Log2(wavelength / (2d * spacing))), 1d, 6d);
        int warpOctaves = (int)Math.Clamp(1d + Math.Floor(Math.Log2(_radius * 1.4d / (2d * spacing))), 1d, 3d);
        _noise = new(seed, wavelength, octaves);
        _crestNoise = new(seed ^ 0x2F6E91, _radius * 1.6d, Math.Min(octaves, 2));
        _warp = new(seed ^ 0x51A7B3, _radius * 1.4d, warpOctaves, _radius * 0.7d);
    }

    public int Apply(Vector2 start, Vector2 end)
    {
        if (_height == 0d) return 0;
        double sx = _field.SpacingXMeters, sy = _field.SpacingYMeters;
        int x0 = Bound(Math.Min(start.X, end.X) - _radius, sx, _field.SamplesX);
        int x1 = Bound(Math.Max(start.X, end.X) + _radius, sx, _field.SamplesX);
        int y0 = Bound(Math.Min(start.Y, end.Y) - _radius, sy, _field.SamplesY);
        int y1 = Bound(Math.Max(start.Y, end.Y) + _radius, sy, _field.SamplesY);
        double ax = (double)end.X - start.X, ay = (double)end.Y - start.Y;
        double lengthSquared = ax * ax + ay * ay;
        int changed = 0;
        for (int y = y0; y <= y1; y++)
        for (int x = x0; x <= x1; x++)
        {
            double px = x * sx, py = y * sy;
            double dx = px - start.X, dy = py - start.Y;
            if (DistanceSquared(dx, dy, ax, ay, lengthSquared) >= _radius * _radius) continue;
            double mx = px - _origin.X, my = py - _origin.Y;
            var (wx, wy) = WarpedPoint(mx, my);
            double distance = Math.Sqrt(DistanceSquared(dx + (wx - mx), dy + (wy - my), ax, ay, lengthSquared));
            double profile = Profile(wx, wy, distance / _radius);
            float candidate = _reference.GetHeight(x, y) + (float)(_height * profile);
            if (candidate <= _field.GetHeight(x, y)) continue;
            _field.SetHeight(x, y, candidate);
            changed++;
        }
        return changed;
    }

    private (double X, double Y) WarpedPoint(double x, double y)
    {
        var p = _warp.Warp(x, y);
        double dx = p.X - x, dy = p.Y - y;
        double length = Math.Sqrt(dx * dx + dy * dy);
        // 位移与最大遮罩半宽之和不超过 R，分段合成不需要额外裁切包络。
        double limit = _radius * 0.18d;
        if (length > limit) { dx *= limit / length; dy *= limit / length; }
        return (x + dx, y + dy);
    }

    private double Profile(double x, double y, double distance)
    {
        if (distance >= 0.82d) return 0d;
        double broad = _crestNoise.SampleRidged(x, y);
        double crest = 0.18d + 0.82d * broad * broad * broad;
        double apron = Envelope(distance, 0.58d + 0.24d * broad, _apronExponent);
        double core = Envelope(distance, 0.55d + 0.27d * crest, _coreExponent);
        double ridge = _noise.SampleRidged(x, y);
        return (0.22d * apron + 0.78d * core * crest) * (0.5d + 0.5d * ridge * ridge);
    }

    private static double Envelope(double distance, double width, double exponent) =>
        Math.Pow(Math.Max(0d, 1d - distance * distance / (width * width)), exponent);

    private static double DistanceSquared(double dx, double dy, double ax, double ay, double lengthSquared)
    {
        double u = lengthSquared == 0d ? 0d : Math.Clamp((dx * ax + dy * ay) / lengthSquared, 0d, 1d);
        double rx = dx - u * ax, ry = dy - u * ay;
        return rx * rx + ry * ry;
    }

    private static int Bound(double meters, double spacing, int samples) =>
        (int)Math.Clamp(Math.Floor(meters / spacing), 0d, samples - 1d);
}