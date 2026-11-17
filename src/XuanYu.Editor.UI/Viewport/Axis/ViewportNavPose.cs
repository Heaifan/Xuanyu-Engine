using System.Numerics;

namespace XuanYu.Editor.UI;

internal readonly record struct ViewportNavPose(
    Vector3 Outward, Vector3 Right, Vector3 Up);