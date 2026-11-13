using System.Numerics;

namespace XuanYu.Editor.Camera;

public sealed partial class ViewportCamera
{
    public bool IsOrthographic { get; private set; }
    public Vector3 ViewRight => new(-MathF.Sin(_yaw), MathF.Cos(_yaw), 0f);
    public Vector3 ViewUp => Vector3.Normalize(Vector3.Cross(
        ViewRight, Vector3.Normalize(Target - Position)));

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
    }
}