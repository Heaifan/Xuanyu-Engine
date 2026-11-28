using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using Transform = XuanYu.World.Transform;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private readonly GeoGizmoOverlay _gizmo = new();
    private readonly TextBlock _gizmoText = new() { FontSize = 11, Foreground = EditorTheme.TextMain };
    private readonly Border _gizmoStatus = new() { IsHitTestVisible = false, IsVisible = false,
        HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Thickness(12, 0, 0, 12), Padding = new Thickness(8, 4),
        Background = Brushes.White, CornerRadius = new CornerRadius(3) };
    private GeoGizmoLayout? _gizmoLayout;
    private GeoGizmoHandle _gizmoHover;
    private Matrix4x4 _gizmoMatrix;
    private (Guid Id, Transform Value)? _gizmoRendered;
    private (uint Width, uint Height) _gizmoSize;
    internal event Action? G07PositionChanged;

    private Matrix4x4 _localGizmoProjection;
    private bool CanUseEntityGizmo => !_wb.IsSimulating && _wb.CurrentId == PageId.World &&
        _wb.Scene.Selected is { Visible: true } item && (item.IsGeographic
            ? G07EntitiesReady && _wb.IsGeoTerrainPreview
            : !_wb.IsGeoTerrainPreview && _wb.Scene.Camera is not null);
    private bool CanShowGizmo => CanUseEntityGizmo && _wb.G07GizmoMode != GeoGizmoMode.Select;

    private void AttachG07Gizmo()
    {
        _gizmoStatus.Child = _gizmoText;
        Children.Insert(2, _gizmo);
        Children.Add(_gizmoStatus);
        PointerCaptureLost += (_, _) => FinishG07Drag(false);
        PointerExited += (_, _) => { _gizmoHover = GeoGizmoHandle.None; RefreshG07GizmoPaint(); };
        LostFocus += (_, _) => FinishG07Drag(false);
        _wb.Changed += OnG07GizmoStateChanged;
    }

    private void OnG07GizmoStateChanged()
    {
        if (_gizmoDrag is { } drag && (!CanShowGizmo || _wb.Scene.SelectedId != drag.Id || drag.Mode != _wb.G07GizmoMode))
            FinishG07Drag(false);
        if (!CanShowGizmo || (_gizmoLayout is { } layout && layout.Mode != _wb.G07GizmoMode))
            DisplayG07Gizmo(false);
    }

    private void CaptureG07Gizmo()
    {
        _gizmoRendered = CanShowGizmo && _wb.Scene.Selected is { } item
            ? (item.Id, _wb.World.Find(item.Id)!.Transform) : null;
    }

    private void CaptureLocalGizmo(uint width, uint height)
    {
        CaptureG07Gizmo();
        _localGizmoProjection = _wb.Scene.ViewProjection(width, height);
    }

    private void DisplayG07Gizmo(bool geographic)
    {
        _gizmoLayout = null;
        if (CanShowGizmo && _gizmoRendered is { } snapshot && _wb.Scene.SelectedId == snapshot.Id &&
            geographic == _wb.Scene.Selected!.IsGeographic && (!geographic || _g07EntityFrame is not null))
        {
            if (geographic && XuanYu.Runtime.WorldRenderBridge.IsBillboard(_wb.Scene.GetRepresentation(snapshot.Id)) &&
                !_g07EntityFrame!.IsAboveHorizon(snapshot.Value.Spatial.ToEcef()))
            { RefreshG07GizmoPaint(); return; }
            _gizmoSize = ((uint)Math.Max(0, Math.Round(Bounds.Width)), (uint)Math.Max(0, Math.Round(Bounds.Height)));
            var mode = _wb.G07GizmoMode;
            var rotation = mode == GeoGizmoMode.Scale ? snapshot.Value.Rotation : Quaternion.Identity;
            var neutral = Transform.FromSpatial(snapshot.Value.Spatial, rotation, Vector3.One);
            _gizmoMatrix = geographic ? _g07EntityFrame!.ModelToClip(neutral) : neutral.ToMatrix() * _localGizmoProjection;
            _gizmoLayout = GeoGizmoLayout.Create(_gizmoMatrix, _gizmoSize.Width, _gizmoSize.Height, mode);
        }
        RefreshG07GizmoPaint();
    }

    private void RefreshG07GizmoPaint()
    {
        _gizmo.SetFrame(_gizmoLayout, _gizmoDrag?.Handle ?? _gizmoHover, _wb.IsGeoTerrainPreview);
        _gizmoStatus.IsVisible = _gizmoDrag is not null || _gizmoLayout is not null;
        if (_gizmoDrag is null) _gizmoText.Text = _wb.G07GizmoMode switch
        {
            GeoGizmoMode.Rotate => _wb.IsGeoTerrainPreview ? "旋转 · 当地 ENU · Ctrl 5° · Shift 微调 · Esc 取消" : "旋转 · 世界 XYZ · Ctrl 5° · Shift 微调 · Esc 取消",
            GeoGizmoMode.Scale => "缩放 · 模型 XYZ · Ctrl 0.1倍 · Shift 微调 · Esc 取消",
            _ => _wb.IsGeoTerrainPreview ? "平移 · 当地 ENU · Ctrl 吸附 · Shift 微调 · Esc 取消" : "平移 · 世界 XYZ · Ctrl 1m · Shift 微调 · Esc 取消"
        };
    }
}