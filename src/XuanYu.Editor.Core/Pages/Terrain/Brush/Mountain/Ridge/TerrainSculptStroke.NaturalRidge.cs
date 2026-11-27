using System.Numerics;

namespace XuanYu.Editor.TerrainEditing;

public sealed partial class TerrainSculptStroke
{
    private TerrainNaturalRidgeStamp? _naturalRidge;

    private int MoveNaturalPeak(Vector2 center, float height)
    {
        if (!_pointApplied)
        {
            _pointApplied = true;
            int x = Math.Clamp((int)MathF.Round(center.X / _field.SpacingXMeters), 0, _field.SamplesX - 1);
            int y = Math.Clamp((int)MathF.Round(center.Y / _field.SpacingYMeters), 0, _field.SamplesY - 1);
            center = new(x * _field.SpacingXMeters, y * _field.SpacingYMeters);
            _naturalRidge = new(_field, ReferenceSurface, _brush, height, _naturalPeakSeed, center);
            _center = center;
            return _naturalRidge.Apply(center, center);
        }
        if (_naturalRidge is not { } ridge) return 0;
        if (_center is Vector2 start)
        {
            AppendPath(start, center);
            _center = center;
            return 0;
        }
        // 拾取中断后保留已排队的线段，但不连接断点两侧。
        _center = center;
        int changed = AdvanceNaturalRidge();
        return changed + ridge.Apply(center, center);
    }

    public int FinishMountain() => AdvanceNaturalRidge();

    private int AdvanceNaturalRidge()
    {
        if (_naturalRidge is not { } ridge) return 0;
        int changed = 0;
        foreach (var (start, end) in _path) changed += ridge.Apply(start, end);
        _path.Clear();
        return changed;
    }
}