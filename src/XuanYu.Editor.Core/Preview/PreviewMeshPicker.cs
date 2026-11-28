using System.Numerics;
using XuanYu.Render;
using XuanYu.Runtime;

namespace XuanYu.Editor.Preview;

public static partial class PreviewMeshPicker
{
    public static Guid? Pick(PreviewScene scene, int x, int y,
        uint width, uint height) => Pick(scene, x, y, width, height,
            scene.ViewProjection(width, height));

    public static Guid? Pick(PreviewScene scene, int x, int y,
        uint width, uint height, Matrix4x4 camera)
    {
        Vector2 point = new(x, y);
        var symbol = PickBillboard(scene, point, width, height, camera);
        if (symbol is not null) return symbol;
        for (int i = scene.Objects.Count - 1; i >= 0; i--)
        {
            PreviewObject item = scene.Objects[i];
            if (!item.Visible || item.IsGeographic ||
                scene.GetActiveMesh(item.Id) is not { } mesh) continue;
            Matrix4x4 transform = scene.LocalDrawMatrix(item.Id,camera,width,height);
            if (WorldRenderBridge.IsBillboard(scene.GetRepresentation(item.Id))) continue;
            if (Hits(mesh,point,transform,width,height)) return item.Id;
        }
        return null;
    }

    internal static bool VisibleBillboard(Matrix4x4 matrix) =>
        WorldRenderBridge.IsBillboardInDepth(matrix);

    internal static bool Hits(RenderMesh mesh,Vector2 point,Matrix4x4 transform,uint width,uint height)
    {
        ReadOnlySpan<float> data=mesh.Vertices.Span;
        var indices=mesh.Indices.Span;
        for (int i=0;i<mesh.TriangleCount*3;i+=3)
        {
            int a=indices.IsEmpty?i*7:(int)indices[i]*7;
            int b=indices.IsEmpty?(i+1)*7:(int)indices[i+1]*7;
            int c=indices.IsEmpty?(i+2)*7:(int)indices[i+2]*7;
            if (Contains(point,Project(data,a,transform,width,height),
                Project(data,b,transform,width,height),Project(data,c,transform,width,height))) return true;
        }
        return false;
    }

    private static Vector2 Project(ReadOnlySpan<float> data, int offset,
        Matrix4x4 matrix, uint width, uint height)
    {
        Vector4 clip = Vector4.Transform(new Vector4(
            data[offset], data[offset + 1], data[offset + 2], 1f), matrix);
        return new Vector2(
            (clip.X / clip.W * 0.5f + 0.5f) * width,
            (clip.Y / clip.W * 0.5f + 0.5f) * height);
    }

    private static bool Contains(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Edge(a, b, c);
        if (MathF.Abs(area) < 0.0001f) return false;
        float ab = Edge(a, b, p);
        float bc = Edge(b, c, p);
        float ca = Edge(c, a, p);
        return (ab >= 0 && bc >= 0 && ca >= 0) ||
               (ab <= 0 && bc <= 0 && ca <= 0);
    }

    private static float Edge(Vector2 a, Vector2 b, Vector2 p) =>
        (b.X - a.X) * (p.Y - a.Y) -
        (b.Y - a.Y) * (p.X - a.X);
}