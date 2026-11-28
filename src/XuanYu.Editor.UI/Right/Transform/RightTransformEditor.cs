using System.Numerics;
using Avalonia.Controls;
using XuanYu.Editor.UI.Left;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.Editor.UI.Right;

internal sealed class RightTransformEditor : StackPanel
{
    private readonly Action<Transform> _apply;
    private Transform _current;
    private Vector3 _degrees;

    public RightTransformEditor(Transform initial, Action<Transform> apply)
    {
        _apply = apply;
        _current = initial;
        _degrees = TransformEuler.ToDegrees(initial.Rotation);
        var group = new RightPropertyGroup("变换",
            (key, size) => LeftSvgIcon.Create(key, size));

        if (initial.Spatial.Space == SpatialSpace.Local)
        {
            group.AddSubheading("位置");
            group.AddNumber("X", initial.Position.X, v => SetPosition(0, v));
            group.AddNumber("Y", initial.Position.Y, v => SetPosition(1, v));
            group.AddNumber("Z", initial.Position.Z, v => SetPosition(2, v));
        }

        group.AddSubheading("旋转（°）");
        group.AddNumber("X", _degrees.X, v => SetRotation(0, v), 1f);
        group.AddNumber("Y", _degrees.Y, v => SetRotation(1, v), 1f);
        group.AddNumber("Z", _degrees.Z, v => SetRotation(2, v), 1f);

        group.AddSubheading("缩放");
        group.AddNumber("X", initial.Scale.X, v => SetScale(0, v));
        group.AddNumber("Y", initial.Scale.Y, v => SetScale(1, v));
        group.AddNumber("Z", initial.Scale.Z, v => SetScale(2, v));
        Children.Add(group);
    }

    private void SetPosition(int axis, float number)
    {
        var p = _current.Spatial;
        var position = SpatialPosition.FromLocal(
            axis == 0 ? number : p.X,
            axis == 1 ? number : p.Y,
            axis == 2 ? number : p.Z);
        _current = Transform.FromSpatial(position, _current.Rotation, _current.Scale);
        _apply(_current);
    }

    private void SetRotation(int axis, float number)
    {
        _degrees = Replace(_degrees, axis, number);
        _current = _current with
        {
            Rotation = TransformEuler.FromDegrees(_degrees)
        };
        _apply(_current);
    }

    private void SetScale(int axis, float number)
    {
        _current = _current with
        {
            Scale = Replace(_current.Scale, axis, number)
        };
        _apply(_current);
    }

    private static Vector3 Replace(Vector3 source, int axis, float number) =>
        axis switch
        {
            0 => source with { X = number },
            1 => source with { Y = number },
            _ => source with { Z = number }
        };
}