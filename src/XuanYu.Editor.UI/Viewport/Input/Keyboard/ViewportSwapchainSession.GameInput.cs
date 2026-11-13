using XuanYu.Editor.Camera;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportSwapchainSession
{
    private void OnGameInput(GameCameraKey key, bool down) =>
        _workbench.SetGameCameraKey(key, down);
}