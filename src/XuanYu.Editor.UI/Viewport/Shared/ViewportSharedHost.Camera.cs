using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private void OnCameraGesture(ViewportCameraGesture gesture)
    {
        if (_wb.CurrentId != PageId.World ||
            _wb.ActiveCamera is not { } camera)
            return;
        if (gesture.WheelSteps != 0)
            camera.Zoom(gesture.WheelSteps);
        else if (gesture.Pan)
            camera.Pan(gesture.DeltaX, gesture.DeltaY,
                Math.Max(1u, PixelSize.Height));
        else
            camera.Orbit(gesture.DeltaX, gesture.DeltaY);
    }

    private void OnCameraShortcut(ViewportCameraShortcut shortcut)
    {
        if (_wb.IsSimulating || _wb.CurrentId != PageId.World ||
            _wb.Scene.Camera is not { } camera)
            return;
        var scene = _wb.Scene;
        var (width, height) = PixelSize;
        float aspect = Math.Max(1u, width) / (float)Math.Max(1u, height);
        if (shortcut == ViewportCameraShortcut.Home)
        {
            bool found = ViewportSceneFraming.FrameAll(
                camera, scene.Objects, aspect);
            _wb.Log.Write(found
                ? "Home：已显示全部可见实体"
                : "Home：场景为空，已恢复默认视角");
        }
        else if (shortcut == ViewportCameraShortcut.FocusSelection)
        {
            bool found = ViewportSceneFraming.Focus(
                camera, scene.Selected, aspect);
            _wb.Log.Write(found
                ? $"F：聚焦实体 {scene.Selected!.Name}"
                : "F：请先选中一个可见实体");
        }
    }
}