using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportSwapchainSession
{
    private void OnCameraGesture(ViewportCameraGesture gesture)
    {
        if (_workbench.CurrentId == PageId.Terrain &&
            _workbench.IsGeoTerrainPreview)
        {
            _workbench.NavigateGeoTerrain(gesture.DeltaX, gesture.DeltaY,
                gesture.WheelSteps, gesture.Pan, _viewport.PixelSize.Height);
            return;
        }
        if (_workbench.CurrentId is not (PageId.World or PageId.Terrain) ||
            _workbench.ActiveCamera is not { } camera)
            return;

        if (gesture.WheelSteps != 0)
            camera.Zoom(gesture.WheelSteps);
        else if (gesture.Pan)
            camera.Pan(gesture.DeltaX, gesture.DeltaY,
                Math.Max(1u, _viewport.PixelSize.Height));
        else
            camera.Orbit(gesture.DeltaX, gesture.DeltaY);
    }
}