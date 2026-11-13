using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportSwapchainSession
{
    private void OnCameraShortcut(ViewportCameraShortcut shortcut)
    {
        if (_workbench.IsSimulating || _workbench.CurrentId != PageId.World ||
            _workbench.Scene.Camera is not { } camera)
            return;

        var scene = _workbench.Scene;
        var (width, height) = _viewport.PixelSize;
        float aspect = Math.Max(1u, width) / (float)Math.Max(1u, height);

        if (shortcut == ViewportCameraShortcut.Home)
        {
            bool found = ViewportSceneFraming.FrameAll(
                camera, scene.Objects, aspect);
            _workbench.Log.Write(found
                ? "Home：已显示全部可见实体"
                : "Home：场景为空，已恢复默认视角");
        }
        else if (shortcut == ViewportCameraShortcut.FocusSelection)
        {
            bool found = ViewportSceneFraming.Focus(
                camera, scene.Selected, aspect);
            _workbench.Log.Write(found
                ? $"F：聚焦实体 {scene.Selected!.Name}"
                : "F：请先选中一个可见实体");
        }
    }
}