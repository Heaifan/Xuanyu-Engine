using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private void OnCameraGesture(ViewportCameraGesture gesture)
    {
        if (_wb.IsSimulating && !_wb.CanNavigateRuntimeCamera) return;
        if (_wb.IsGeoTerrainPreview)
        {
            if (gesture.WheelSteps != 0f)
            {
                var (width, height) = PixelSize;
                _wb.ZoomGeoTerrainAt(gesture.WheelSteps,
                    gesture.CursorX, gesture.CursorY, width, height);
            }
            else _wb.NavigateGeoTerrain(gesture.DeltaX, gesture.DeltaY,
                0f, gesture.Pan, PixelSize.Height);
            return;
        }
        if (_wb.ActiveCamera is not { } camera) return;
        if (gesture.WheelSteps != 0)
        {
            if (!_wb.IsSimulating)
            {
                var (width, height) = PixelSize;
                _wb.ZoomTerrainAt(gesture.WheelSteps,
                    gesture.CursorX, gesture.CursorY, width, height);
            }
            else camera.Zoom(gesture.WheelSteps);
        }
        else if (gesture.Pan)
            camera.Pan(gesture.DeltaX, gesture.DeltaY,
                Math.Max(1u, PixelSize.Height));
        else camera.Orbit(gesture.DeltaX, gesture.DeltaY);
    }

    private void OnCameraShortcut(ViewportCameraShortcut shortcut)
    {
        if (_wb.IsSimulating)
        {
            if (_wb.IsGeoTerrainPreview && _wb.CanNavigateRuntimeCamera &&
                shortcut == ViewportCameraShortcut.Home) _wb.ResetGeoTerrainCamera();
            return;
        }
        if (_wb.IsGeoTerrainPreview)
        {
            if (shortcut == ViewportCameraShortcut.FocusSelection && _wb.CurrentId == PageId.World)
            {
                if (!_wb.FocusG07Entity()) _wb.Log.Write("F：请先选中一个可见全球实体");
            }
            else _wb.ResetGeoTerrainCamera();
            return;
        }
        var scene = _wb.CurrentId == PageId.Terrain ? _wb.TerrainScene : _wb.Scene;
        if (_wb.ActiveCamera is not { } camera) return;
        var (width, height) = PixelSize;
        float aspect = Math.Max(1u, width) / (float)Math.Max(1u, height);
        if (shortcut == ViewportCameraShortcut.Home ||
            _wb.CurrentId == PageId.Terrain)
        {
            bool found = ViewportSceneFraming.FrameAll(camera,
                scene.Objects, aspect);
            _wb.Log.Write(found ? "[E5/视口] Home：已聚焦地形"
                : "[E5/视口] Home：已恢复初始视角");
        }
        else
        {
            bool found = ViewportSceneFraming.Focus(
                camera, scene.Selected, aspect);
            _wb.Log.Write(found ? $"F：聚焦实体 {scene.Selected!.Name}"
                : "F：请先选中一个可见实体");
        }
    }
}