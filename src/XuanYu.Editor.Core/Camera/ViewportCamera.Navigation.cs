using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed partial class ViewportCamera
{
    private const float TerrainClearance = 0.5f;
    private Func<Vector2, float?>? _terrainHeight;
    private (Vector3 Min, Vector3 Max)? _localTerrainBounds;

    public bool IsOrthographic { get; private set; }
    public Vector3 ViewRight => new(-MathF.Sin(_yaw), MathF.Cos(_yaw), 0f);
    public Vector3 ViewUp => Vector3.Normalize(Vector3.Cross(
        ViewRight, Vector3.Normalize(Target - Position)));

    public void SetTerrainCollision(Func<Vector2, float?> sampleHeight)
    {
        ArgumentNullException.ThrowIfNull(sampleHeight);
        _terrainHeight = sampleHeight;
        _localTerrainBounds = null;
        LiftAboveTerrain();
    }

    private bool IsAboveTerrain()
    {
        Vector3 position = Position;
        float? height = _terrainHeight?.Invoke(new Vector2(position.X, position.Y));
        return height is null || position.Z >= height.Value + TerrainClearance;
    }

    internal void LiftAboveTerrain()
    {
        Vector3 position = Position;
        float? height = _terrainHeight?.Invoke(new Vector2(position.X, position.Y));
        if (height is { } z && position.Z < z + TerrainClearance)
            Target += new Vector3(0f, 0f, z + TerrainClearance - position.Z);
    }

    private void ConstrainZoom(float previous)
    {
        if (_terrainHeight is null || previous == _distance) return;
        float requested = _distance;
        float allowed = previous;
        for (int i = 1; i <= 32; i++)
        {
            _distance = previous + (requested - previous) * (i / 32f);
            if (IsAboveTerrain()) { allowed = _distance; continue; }
            float blocked = _distance;
            for (int n = 0; n < 16; n++)
            {
                _distance = (allowed + blocked) * 0.5f;
                if (IsAboveTerrain()) allowed = _distance;
                else blocked = _distance;
            }
            _distance = allowed;
            return;
        }
        _distance = requested;
    }

    internal void SetLocalTerrainBounds(Vector3 min, Vector3 max) =>
        _localTerrainBounds = (min, max);

    private (float Near, float Far) ClipDepthRange()
    {
        float far = MathF.Max(1000f, _distance * 4f);
        if (!IsOrthographic || _localTerrainBounds is not { } bounds)
            return (0.1f, far);
        Vector3 forward = Vector3.Normalize(Target - Position);
        Vector3 center = (bounds.Min + bounds.Max) * 0.5f;
        Vector3 half = (bounds.Max - bounds.Min) * 0.5f;
        float depth = Vector3.Dot(center - Position, forward);
        float reach = Vector3.Dot(half, Vector3.Abs(forward));
        // 正交深度允许覆盖逻辑相机身后，不改变 XY 投影尺寸。
        return (MathF.Min(0.1f, depth - reach - 1f), MathF.Max(far, depth + reach + 1f));
    }

    public void ToggleProjection() => IsOrthographic = !IsOrthographic;

    public void SnapAxis(Vector3 axis)
    {
        if (axis.Z != 0f)
        {
            _yaw = axis.Z > 0f ? -MathF.PI / 2f : MathF.PI / 2f;
            _pitch = axis.Z > 0f ? MathF.PI / 2f : -MathF.PI / 2f;
        }
        else
        {
            _yaw = MathF.Atan2(axis.Y, axis.X);
            _pitch = 0f;
        }
        IsOrthographic = true;
        LiftAboveTerrain();
    }
}