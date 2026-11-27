using System.Numerics;

namespace XuanYu.Editor.TerrainEditing;

public sealed partial class TerrainSculptStroke
{
    private void AppendPath(Vector2 start, Vector2 end)
    {
        if (start == end) return;
        if (_path.Count > 0 && _path[^1].End == start)
        {
            var last = _path[^1];
            Vector2 a = start - last.Start, b = end - start;
            if (a.X * b.Y == a.Y * b.X && Vector2.Dot(a, b) > 0f)
            {
                _path[^1] = (last.Start, end);
                return;
            }
        }
        _path.Add((start, end));
    }

    public int Advance(float seconds)
    {
        if (_naturalPeakHeight.HasValue) return AdvanceNaturalRidge();
        if (_target.HasValue && !_area) return 0;
        if (!_target.HasValue && seconds <= 0f) return 0;
        float delta = _direction * _brush.Strength * _brush.RadiusMeters * 0.2f * seconds;
        if (_path.Count == 0)
            return !_target.HasValue && _center is Vector2 center
                ? _flattenHeight.HasValue ? FlattenForTime(center, seconds) :
                    _rain ? RainForTime(center, seconds) :
                    _thermal ? ThermalForTime(center, seconds) :
                    _smooth ? SmoothForTime(center, seconds) :
                     TerrainHeightStamp.ApplyDelta(_field, center, _brush, delta) : 0;

        float distance = 0f;
        foreach (var (start, end) in _path) distance += Vector2.Distance(start, end);
        float spacing = MathF.Min(_field.SpacingXMeters, _field.SpacingYMeters);
        float step = MathF.Max(_brush.RadiusMeters * 0.25f, spacing * 0.5f);
        int changed = 0;
        foreach (var (start, end) in _path)
        {
            float length = Vector2.Distance(start, end);
            int count = Math.Max(1, (int)MathF.Ceiling(length / step));
            float share = delta * (length / distance) / count;
            float time = seconds * (length / distance) / count;
            for (int i = 1; i <= count; i++)
            {
                Vector2 point = Vector2.Lerp(start, end, (float)i / count);
                changed += _target is float target ? SetElevation(point, target) :
                    _flattenHeight.HasValue ? FlattenForTime(point, time) :
                    _rain ? RainForTime(point, time) :
                    _thermal ? ThermalForTime(point, time) :
                    _smooth ? SmoothForTime(point, time) :
                    TerrainHeightStamp.ApplyDelta(_field, point, _brush, share);
            }
        }
        _path.Clear();
        return changed;
    }

    private int ThermalForTime(Vector2 point, float seconds)
    {
        float strength = 1f - MathF.Exp(-30f * _brush.Strength * seconds);
        return TerrainThermalStamp.Apply(_field, point, _brush, strength);
    }

    private int FlattenForTime(Vector2 point, float seconds)
    {
        float strength = 1f - MathF.Exp(-12f * _brush.Strength * seconds);
        return TerrainFlattenStamp.Apply(_field, point, _brush, _flattenHeight!.Value, strength);
    }

    private int SmoothForTime(Vector2 point, float seconds)
    {
        // 强度按经过时间衰减，不随定时器或路径分段次数线性放大。
        float strength = 1f - MathF.Exp(-12f * _brush.Strength * seconds);
        return TerrainSmoothStamp.Apply(_field, point, _brush, strength);
    }
}