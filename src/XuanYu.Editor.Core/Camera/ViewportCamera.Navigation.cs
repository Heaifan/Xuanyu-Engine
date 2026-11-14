
using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed partial class ViewportCamera
{
    private const float TerrainClearance = 0.5f;
    private Func<Vector2, float?>? _terrainHeight;

    public bool IsOrthographic { get; private set; }
    public Vector3 ViewRight => new(-MathF.Sin(_yaw), MathF.Cos(_yaw), 0f);
    public Vector3 ViewUp => Vector3.Normalize(Vector3.Cross(
        ViewRight, Vector3.Normalize(Target - Position)));

    public void SetTerrainCollision(Func<Vector2, float?> sampleHeight)
    {
        ArgumentNullException.ThrowIfNull(sampleHeight);
        _terrainHeight = sampleHeight;
        LiftAboveTerrain();
    }

    private bool IsAboveTerrain()
    {
        Vector3 position = Position;
        float? height = _terrainHeight?.Invoke(new Vector2(position.X, position.Y));
        return height is null || position.Z >= height.Value + TerrainClearance;
    }

    private void LiftAboveTerrain()
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
