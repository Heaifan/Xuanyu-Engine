
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportSwapchainSession
{
    private void OnCameraShortcut(ViewportCameraShortcut shortcut)
    {
        if (_workbench.IsSimulating ||
            _workbench.CurrentId is not (PageId.World or PageId.Terrain))
            return;

        PreviewScene scene = _workbench.CurrentId == PageId.Terrain
            ? _workbench.TerrainScene : _workbench.Scene;

        if (scene.Camera is not { } camera)
            return;

        var (width, height) = _viewport.PixelSize;
        float aspect = Math.Max(1u, width) /
            (float)Math.Max(1u, height);

        bool terrain = _workbench.CurrentId == PageId.Terrain;

        if (shortcut == ViewportCameraShortcut.Home ||
            (terrain && shortcut == ViewportCameraShortcut.FocusSelection))
        {
            bool found = ViewportSceneFraming.FrameAll(
                camera, scene.Objects, aspect);

            _workbench.Log.Write(terrain
                ? "E4 地形：已聚焦完整地图"
                : found
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
