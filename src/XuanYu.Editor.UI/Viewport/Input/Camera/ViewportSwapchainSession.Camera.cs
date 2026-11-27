using XuanYu.Editor.Pages;

namespace XuanYu.Editor.UI;

internal sealed partial class ViewportSwapchainSession
{
    private void OnCameraGesture(ViewportCameraGesture gesture)
    {
        if (_workbench.IsSimulating && _workbench.CurrentId != PageId.World) return;
        if (_workbench.IsGeoTerrainPreview && !_workbench.IsSimulating)
        {
            if (gesture.WheelSteps != 0f)
            {
                var (width, height) = _viewport.PixelSize;
                _workbench.ZoomGeoTerrainAt(gesture.WheelSteps,
                    gesture.CursorX, gesture.CursorY, width, height);
            }
            else _workbench.NavigateGeoTerrain(gesture.DeltaX, gesture.DeltaY,
                0f, gesture.Pan, _viewport.PixelSize.Height);
            return;
        }
        if (_workbench.ActiveCamera is not { } camera) return;
        if (gesture.WheelSteps != 0)
        {
            if (!_workbench.IsSimulating)
            {
                var (width, height) = _viewport.PixelSize;
                _workbench.ZoomTerrainAt(gesture.WheelSteps,
                    gesture.CursorX, gesture.CursorY, width, height);
            }
            else camera.Zoom(gesture.WheelSteps);
        }
        else if (gesture.Pan)
            camera.Pan(gesture.DeltaX, gesture.DeltaY,
                Math.Max(1u, _viewport.PixelSize.Height));
        else camera.Orbit(gesture.DeltaX, gesture.DeltaY);
    }
}