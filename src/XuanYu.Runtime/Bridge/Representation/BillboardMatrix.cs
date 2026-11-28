using System.Numerics;

namespace XuanYu.Runtime;

internal static class BillboardMatrix
{
    public static Matrix4x4 Create(Vector3 position, Matrix4x4 projection,
        BillboardStyle style, uint width, uint height)
    {
        var origin = Vector4.Transform(new Vector4(position,1),projection);
        // 源网格使用 SVG 向下的 Y 轴；Vulkan 正高度视口也向下。
        float x,y;
        if (style.FixedPixels && width>0 && height>0)
        {
            x=2*style.Size*origin.W/width; y=2*style.Size*origin.W/height;
        }
        else
        {
            float size=style.FixedPixels ? 1 : style.Size;
            x=size*new Vector3(projection.M11,projection.M21,projection.M31).Length();
            y=size*new Vector3(projection.M12,projection.M22,projection.M32).Length();
        }
        return new Matrix4x4(x,0,0,0, 0,y,0,0, 0,0,1,0,
            origin.X,origin.Y,origin.Z,origin.W);
    }
}