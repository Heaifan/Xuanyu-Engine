using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.TerrainEditing;

public sealed partial class TerrainSculptStroke
{
    private readonly TerrainHeightField _field;
    private readonly TerrainBrushSettings _brush;
    private readonly float _direction;
    private readonly float? _target;
    private readonly bool _area;
    private readonly bool _smooth, _thermal, _rain;
    private readonly float? _flattenHeight;
    private readonly float? _naturalPeakHeight;
    private readonly int _naturalPeakSeed;
    private Vector2? _center;
    private readonly List<(Vector2 Start, Vector2 End)> _path = [];
    private bool _pointApplied;
    internal TerrainHeightField ReferenceSurface { get; }

    public TerrainSculptStroke(TerrainHeightField field,
        TerrainBrushSettings brush, bool lower, float? target = null, bool area = false,
        bool smooth = false, float? flattenHeight = null,
        float? naturalPeakHeight = null, int naturalPeakSeed = 7429,
        bool thermal = false, bool rain = false)
    {
        _field = field;
        _brush = brush;
        _direction = lower ? -1f : 1f;
        _target = target;
        _area = area;
        _smooth = smooth; _thermal = thermal; _rain = rain;
        _flattenHeight = flattenHeight;
        _naturalPeakHeight = naturalPeakHeight; _naturalPeakSeed = naturalPeakSeed;
        ReferenceSurface = new TerrainHeightField(field.SamplesX, field.SamplesY,
            field.SpacingXMeters, field.SpacingYMeters);
        for (int y = 0; y < field.SamplesY; y++)
        for (int x = 0; x < field.SamplesX; x++)
            ReferenceSurface.SetHeight(x, y, field.GetHeight(x, y));
    }

    public int MoveTo(Vector2 center)
    {
        if (_naturalPeakHeight is float peak) return MoveNaturalPeak(center, peak);
        Vector2? previous = _center;
        _center = center;
        if (previous is Vector2 start)
        {
            if (!_target.HasValue || _area) AppendPath(start, center);
            return 0;
        }
        if (_target.HasValue && !_area)
        {
            if (_pointApplied) return 0;
            _pointApplied = true;
        }
        return _target is float target ? SetElevation(center, target) :
            _flattenHeight is float level ? TerrainFlattenStamp.Apply(_field, center, _brush, level) :
            _rain ? RainForTime(center, 1f / 30f) :
            _thermal ? TerrainThermalStamp.Apply(_field, center, _brush) :
            _smooth ? TerrainSmoothStamp.Apply(_field, center, _brush) :
            TerrainHeightStamp.ApplyDelta(_field, center, _brush,
                _direction * _brush.Strength * MathF.Max(1f, _brush.RadiusMeters / 10f));
    }

    public void BreakPath() => _center = null;

    private int SetElevation(Vector2 center, float target)
    {
        int x = Math.Clamp((int)MathF.Round(center.X / _field.SpacingXMeters), 0, _field.SamplesX - 1);
        int y = Math.Clamp((int)MathF.Round(center.Y / _field.SpacingYMeters), 0, _field.SamplesY - 1);
        center = new(x * _field.SpacingXMeters, y * _field.SpacingYMeters);
        var brush = _area ? _brush : new TerrainBrushSettings(_brush.RadiusMeters, 1f, 1f);
        return TerrainHeightStamp.Visit(_field, center, brush, (sx, sy, weight) =>
        {
            float original = ReferenceSurface.GetHeight(sx, sy);
            float candidate = weight == 1f ? target : float.Lerp(original, target, weight);
            float current = _field.GetHeight(sx, sy);
            return target >= original ? MathF.Max(current, candidate) : MathF.Min(current, candidate);
        });
    }
}