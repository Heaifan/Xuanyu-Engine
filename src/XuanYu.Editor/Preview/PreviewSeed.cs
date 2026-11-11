
using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public static class PreviewSeed
{
    public static PreviewScene Create()
    {
        var scene = new PreviewScene();

        var mesh = new RenderMesh(
            "demo/colored-face",
            new float[]
            {
                -0.7f, 0.6f, 0f, 1f, 0.25f, 0.25f, 1f,
                 0.7f, 0.6f, 0f, 0.25f, 1f, 0.45f, 1f,
                 0f, -0.7f, 0f, 0.25f, 0.55f, 1f, 1f
            });

        var quad = new RenderMesh(
            "demo/quad",
            new float[]
            {
                -0.6f, -0.6f, 0f, 0.3f, 0.85f, 1f, 1f,
                 0.6f, -0.6f, 0f, 0.3f, 0.85f, 1f, 1f,
                 0.6f,  0.6f, 0f, 0.4f, 1f, 0.6f, 1f,
                -0.6f, -0.6f, 0f, 0.3f, 0.85f, 1f, 1f,
                 0.6f,  0.6f, 0f, 0.4f, 1f, 0.6f, 1f,
                -0.6f,  0.6f, 0f, 0.4f, 1f, 0.6f, 1f
            });

        Matrix4x4 scale = Matrix4x4.CreateScale(0.55f);

        scene.Add(
            "彩色三角面",
            mesh,
            scale * Matrix4x4.CreateTranslation(-0.45f, 0, 0));

        scene.Add(
            "彩色四边形",
            quad,
            scale * Matrix4x4.CreateTranslation(0.45f, 0, 0));

        return scene;
    }
}
