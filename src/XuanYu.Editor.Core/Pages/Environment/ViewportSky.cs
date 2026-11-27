using System.Numerics;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public static class ViewportSky
{
    private static readonly RenderMesh SkyMesh = CreateMesh();

    public static RenderDraw WithPitch(double pitch)
    {
        float offset = (float)Math.Clamp(Math.Sin(pitch) * 0.55d, -0.55d, 0.55d);
        return new RenderDraw(SkyMesh,
            Matrix4x4.CreateTranslation(0f, offset, 0f), true, false);
    }

    private static RenderMesh CreateMesh()
    {
        float[] ys = [-3f, -1f, -0.35f, 0.12f, 1f, 3f];
        Vector3[] colors =
        [
            new(0.64f, 0.78f, 0.91f),
            new(0.64f, 0.78f, 0.91f),
            new(0.79f, 0.86f, 0.93f),
            new(0.46f, 0.70f, 0.91f),
            new(0.12f, 0.32f, 0.66f),
            new(0.12f, 0.32f, 0.66f)
        ];
        var vertices = new List<float>();
        for (int i = 0; i < ys.Length - 1; i++)
        {
            Add(-1f, ys[i], colors[i]);
            Add(1f, ys[i], colors[i]);
            Add(1f, ys[i + 1], colors[i + 1]);
            Add(-1f, ys[i], colors[i]);
            Add(1f, ys[i + 1], colors[i + 1]);
            Add(-1f, ys[i + 1], colors[i + 1]);
        }
        return new RenderMesh("editor/sky-gradient", vertices.ToArray());

        void Add(float x, float y, Vector3 color)
        {
            vertices.AddRange([x, y, 0.9999999f,
                color.X, color.Y, color.Z, 1f]);
        }
    }
}