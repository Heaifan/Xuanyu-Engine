using System.Numerics;
using XuanYu.Editor.Preview;
using XuanYu.Core.Geodesy;
using XuanYu.World;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public GeoGizmoMode G07GizmoMode { get; private set; } = GeoGizmoMode.Translate;
    public bool G07TranslateMode => G07GizmoMode == GeoGizmoMode.Translate;
    public bool G07GroundTranslation { get; private set; } = true;

    public void SetG07TranslateMode(bool enabled) =>
        SetG07GizmoMode(enabled ? GeoGizmoMode.Translate : GeoGizmoMode.Select);

    public void SetG07GizmoMode(GeoGizmoMode mode)
    {
        if (G07GizmoMode == mode) return;
        G07GizmoMode = mode;
        Changed?.Invoke();
    }

    public void ToggleG07GroundTranslation()
    {
        G07GroundTranslation = !G07GroundTranslation;
        Changed?.Invoke();
    }

    public GeoSampleStatus DragG07Entity(GeoUnifiedLodSession cpu, Guid id,
        Transform original, EnuPosition delta, bool ground)
    {
        var (item, current) = EditableGeoEntity(id);
        var next = Translated(original, delta);
        if (ground && delta != default)
        {
            var status = ResolveGround(cpu, item, next, out next);
            if (status != GeoSampleStatus.Valid) return status;
        }
        if (next != current) Scene.SetTransform(id, next);
        return GeoSampleStatus.Valid;
    }

    public void DragG07Orientation(Guid id, Transform original, GeoGizmoMode mode, Vector3 value)
    {
        var (_, current) = EditableGeoEntity(id);
        var next = mode == GeoGizmoMode.Rotate ? Rotated(original, value) : Scaled(original, value);
        if (next != current) Scene.SetTransform(id, next);
    }

    private static Transform Translated(Transform original, EnuPosition delta)
    {
        if (delta == default) return original;
        var frame = new EnuFrame(original.Spatial.ToGeo());
        return Transform.FromSpatial(SpatialPosition.FromEnu(frame, delta), original.Rotation, original.Scale);
    }
}