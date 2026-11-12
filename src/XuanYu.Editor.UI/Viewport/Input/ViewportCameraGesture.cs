namespace XuanYu.Editor.UI;

internal readonly record struct ViewportCameraGesture(
    float DeltaX,
    float DeltaY,
    float WheelSteps,
    bool Pan);