using System.Numerics;
using Avalonia.Input;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.World;
using XuanYu.World.Maps;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private sealed record GizmoDrag(Guid Id, Transform Original, GeoGizmoMode Mode,
        GeoGizmoHandle Handle, GeoGizmoDrag? Math, GeoGizmoOrientationDrag? Orientation,
        IPointer Pointer, (uint Width, uint Height) Size, bool Ground);
    private GizmoDrag? _gizmoDrag;

    private bool TryG07GizmoPress(PointerPressedEventArgs e)
    {
        if (_gizmoDrag is not null) return true;
        if (_panMode || !CanShowGizmo || _gizmoLayout is not { } layout ||
            !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || _gizmoRendered is not { } snapshot ||
            _wb.Scene.SelectedId != snapshot.Id || _wb.World.Find(snapshot.Id)!.Transform != snapshot.Value) return false;
        var p = e.GetPosition(this); var pixel = new Vector2((float)p.X, (float)p.Y);
        var handle = layout.Pick(pixel);
        if (handle == GeoGizmoHandle.None || layout.Mode != _wb.G07GizmoMode) return false;
        var math = layout.Mode == GeoGizmoMode.Translate ?
            GeoGizmoDrag.Begin(_gizmoMatrix, _gizmoSize.Width, _gizmoSize.Height, handle, pixel) : null;
        if (layout.Mode == GeoGizmoMode.Translate && math is null) return true;
        var orientation = math is null ? new GeoGizmoOrientationDrag(layout, handle, pixel) : null;
        _gizmoDrag = new(snapshot.Id, snapshot.Value, layout.Mode, handle, math, orientation, e.Pointer, _gizmoSize,
            _wb.G07GroundTranslation && !handle.HasFlag(GeoGizmoHandle.U));
        Focus(); e.Pointer.Capture(this);
        RefreshG07GizmoPaint();
        return true;
    }

    private bool TryG07GizmoMove(PointerEventArgs e)
    {
        var p = e.GetPosition(this); var pixel = new Vector2((float)p.X, (float)p.Y);
        if (_gizmoDrag is not { } drag)
        {
            var hover = !_dragging && ReferenceEquals(e.Source, this) ?
                _gizmoLayout?.Pick(pixel) ?? GeoGizmoHandle.None : GeoGizmoHandle.None;
            if (hover != _gizmoHover) { _gizmoHover = hover; RefreshG07GizmoPaint(); }
            return false;
        }
        if (!ReferenceEquals(drag.Pointer, e.Pointer)) return true;
        if (!CanShowGizmo || _wb.Scene.SelectedId != drag.Id || drag.Size != _gizmoSize)
        { FinishG07Drag(false); return true; }
        bool fine = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        bool snap = !fine && e.KeyModifiers.HasFlag(KeyModifiers.Control);
        if (drag.Orientation is { } orientation)
        {
            var value = orientation.Value(pixel, fine, snap);
            _wb.DragG07Orientation(drag.Id, drag.Original, drag.Mode, value);
            _gizmoText.Text = drag.Mode == GeoGizmoMode.Rotate ?
                $"旋转 · E {value.X:0.0}° · N {value.Y:0.0}° · U {value.Z:0.0}°" :
                $"缩放 · X {value.X:0.000}倍 · Y {value.Y:0.000}倍 · Z {value.Z:0.000}倍";
            return true;
        }
        if (!drag.Math!.TryDelta(pixel, out var delta)) return true;
        double Adjust(double v, double step) => fine ? v * .2 : snap ? Math.Round(v / step) * step : v;
        delta = new(Adjust(delta.East, 10), Adjust(delta.North, 10), Adjust(delta.Up, 5));
        var result = _wb.DragG07Entity(_g07Cpu!, drag.Id, drag.Original, delta, drag.Ground);
        _gizmoText.Text = result == GeoSampleStatus.Valid ?
            $"平移 · E {delta.East:+0.000;-0.000;0.000}m · N {delta.North:+0.000;-0.000;0.000}m · U {delta.Up:+0.000;-0.000;0.000}m" +
                (drag.Ground ? " · 贴地" : "") : $"贴地失败：{result}，保留最后有效位置";
        if (result == GeoSampleStatus.Valid) G07PositionChanged?.Invoke();
        return true;
    }

    private bool TryG07GizmoRelease(PointerReleasedEventArgs e)
    {
        if (_gizmoDrag is not { } drag) return false;
        if (ReferenceEquals(drag.Pointer, e.Pointer) && e.InitialPressMouseButton == MouseButton.Left)
            FinishG07Drag(true);
        return true;
    }

    private void FinishG07Drag(bool commit)
    {
        if (_gizmoDrag is not { } drag) return;
        _gizmoDrag = null;
        if (!commit && _wb.World.Find(drag.Id) is { } entity && entity.Transform != drag.Original)
            _wb.Scene.SetTransform(drag.Id, drag.Original);
        if (ReferenceEquals(drag.Pointer.Captured, this)) drag.Pointer.Capture(null);
        _gizmoHover = GeoGizmoHandle.None;
        G07PositionChanged?.Invoke();
        RefreshG07GizmoPaint();
        if (drag.Mode != GeoGizmoMode.Translate) G07TransformCommitted?.Invoke();
        string name = drag.Mode == GeoGizmoMode.Rotate ? "旋转" : drag.Mode == GeoGizmoMode.Scale ? "缩放" : "平移";
        _wb.Log.Write($"[E6/Gizmo] {name}{(commit ? "确认" : "取消")}");
    }
}