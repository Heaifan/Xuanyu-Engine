using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public static class PreviewSeed
{
    public static PreviewScene Create()
    {
        var scene = new PreviewScene();
        Matrix4x4 scale = Matrix4x4.CreateScale(0.55f);
        scene.Add("彩色三角面", CreateMesh("demo/colored-face"),
            scale * Matrix4x4.CreateTranslation(-0.45f, 0, 0));
        scene.Add("彩色四边形", CreateMesh("demo/quad"),
            scale * Matrix4x4.CreateTranslation(0.45f, 0, 0));
        return scene;
    }

    public static PreviewScene Create3D()
    {
        var scene = new PreviewScene { Camera = new ViewportCamera() };
        scene.Add("Cube_001", CreateMesh("builtin/cube"), Matrix4x4.Identity);
        return scene;
    }

    internal static RenderMesh CreateMesh(string id) => id switch
    {
        "builtin/cube" => PreviewCubeMesh.Create(),
        "demo/colored-face" => new RenderMesh(id,
            new float[]
            {
                -0.7f, 0.6f, 0f, 1f, 0.25f, 0.25f, 1f,
                 0.7f, 0.6f, 0f, 0.25f, 1f, 0.45f, 1f,
                 0f, -0.7f, 0f, 0.25f, 0.55f, 1f, 1f
            }),
        "demo/quad" => new RenderMesh(id,
            new float[]
            {
                -0.6f, -0.6f, 0f, 0.3f, 0.85f, 1f, 1f,
                 0.6f, -0.6f, 0f, 0.3f, 0.85f, 1f, 1f,
                 0.6f,  0.6f, 0f, 0.4f, 1f, 0.6f, 1f,
                -0.6f, -0.6f, 0f, 0.3f, 0.85f, 1f, 1f,
                 0.6f,  0.6f, 0f, 0.4f, 1f, 0.6f, 1f,
                -0.6f,  0.6f, 0f, 0.4f, 1f, 0.6f, 1f
            }),
        _ => throw new InvalidDataException($"不支持的网格资产：{id}")
    };
}