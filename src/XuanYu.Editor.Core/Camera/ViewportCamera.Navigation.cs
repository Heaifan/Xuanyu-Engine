using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed partial class ViewportCamera
{
    private const double TerrainClearance = 0.5d;
    private Func<Vector2, float?>? _terrainHeight;
    private (Vector3 Min, Vector3 Max)? _localTerrainBounds;
    public bool IsOrthographic { get; private set; }

    public void SetTerrainCollision(Func<Vector2, float?> sampleHeight)
    {
        ArgumentNullException.ThrowIfNull(sampleHeight);
        _terrainHeight = sampleHeight;
        _localTerrainBounds = null;
        LiftAboveTerrain();
    }

    private bool IsAboveTerrain()
    {
        var position = PositionMeters;
        float? height = _terrainHeight?.Invoke(new Vector2((float)position.X, (float)position.Y));
        return height is null || position.Z >= height.Value + TerrainClearance;
    }

    internal void LiftAboveTerrain()
    {
        var position = PositionMeters;
        float? height = _terrainHeight?.Invoke(new Vector2((float)position.X, (float)position.Y));
        if (height is { } z && position.Z < z + TerrainClearance)
            _target.Z += z + TerrainClearance - position.Z;
    }

    private void ConstrainZoom(double previous)
    {
        if (_terrainHeight is null || previous == _distance) return;
        double requested = _distance;
        double allowed = previous;
        for (int i = 1; i <= 32; i++)
        {
            _distance = previous + (requested - previous) * (i / 32d);
            if (IsAboveTerrain()) { allowed = _distance; continue; }
            double blocked = _distance;
            for (int n = 0; n < 16; n++)
            {
                _distance = (allowed + blocked) * 0.5d;
                if (IsAboveTerrain()) allowed = _distance;
                else blocked = _distance;
            }
            _distance = allowed;
            return;
        }
        _distance = requested;
    }

    internal void SetLocalTerrainBounds(Vector3 min, Vector3 max) => _localTerrainBounds = (min, max);

    private (float Near, float Far) ClipDepthRange()
    {
        double far = Math.Max(1000d, _distance * 4d);
        if (!IsOrthographic || _localTerrainBounds is not { } bounds) return (0.1f, (float)far);
        var b = Backward;
        var eye = PositionMeters;
        var min = bounds.Min;
        var max = bounds.Max;
        var center = (((double)min.X + max.X) * .5d, ((double)min.Y + max.Y) * .5d,
            ((double)min.Z + max.Z) * .5d);
        double depth = Dot((eye.X - center.Item1, eye.Y - center.Item2, eye.Z - center.Item3), b);
        double reach = (((double)max.X - min.X) * Math.Abs(b.X) +
            ((double)max.Y - min.Y) * Math.Abs(b.Y) + ((double)max.Z - min.Z) * Math.Abs(b.Z)) * .5d;
        // 正交深度允许覆盖逻辑相机身后，不改变 XY 投影尺寸。
        return ((float)Math.Min(.1d, depth - reach - 1d), (float)Math.Max(far, depth + reach + 1d));
    }

    public void ToggleProjection() => IsOrthographic = !IsOrthographic;

    public void SnapAxis(Vector3 axis)
    {
        if (axis.Z != 0f)
        {
            _yaw = axis.Z > 0f ? -Math.PI / 2d : Math.PI / 2d;
            _pitch = axis.Z > 0f ? Math.PI / 2d : -Math.PI / 2d;
        }
        else
        {
            _yaw = Math.Atan2(axis.Y, axis.X);
            _pitch = 0d;
        }
        IsOrthographic = true;
        LiftAboveTerrain();
    }
}