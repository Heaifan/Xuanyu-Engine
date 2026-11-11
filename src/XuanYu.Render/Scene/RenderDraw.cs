
using System.Numerics;

namespace XuanYu.Render;

public readonly record struct RenderDraw(
    RenderMesh Mesh,
    Matrix4x4 ModelToClip,
    bool Visible,
    bool Selected);
