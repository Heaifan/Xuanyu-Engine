using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;

namespace XuanYu.Editor.Preview;

public sealed class MetricGridOverlay
{
    private (double Step, long X, long Y) _key;
    private RenderMesh? _mesh;

    public RenderDraw Draw(ViewportCamera camera, Matrix4x4 projection)
    {
        double distance = camera.DistanceMeters;
        double step = GridStep.FromDistance(distance);
        long x = (long)Math.Round(camera.TargetMeters.X / (8d * step));
        long y = (long)Math.Round(camera.TargetMeters.Y / (8d * step));
        var key = (step, x, y);
        if (_mesh is null || _key != key)
        {
            _key = key;
            _mesh = Build(step, x * 8d * step, y * 8d * step);
        }
        return new RenderDraw(_mesh!, projection, true, false);
    }

    private static RenderMesh Build(double step, double cx, double cy)
    {
        const int half = 64, stride = 8;
        var vertices = new List<float>(90_000);
        Vector3 fog = new(0.64f, 0.78f, 0.91f);
        for (int axis = 0; axis < 2; axis++)
        for (int i = -half; i <= half; i++)
        for (int j = -half; j < half; j += stride)
        {
            double x0 = axis == 0 ? cx + i * step : cx + j * step;
            double y0 = axis == 0 ? cy + j * step : cy + i * step;
            double x1 = axis == 0 ? x0 : x0 + stride * step;
            double y1 = axis == 0 ? y0 + stride * step : y0;
            Vector3 baseColor = axis == 0 && x0 == 0d ? new(0.73f, 0.33f, 0.31f)
                : axis == 1 && y0 == 0d ? new(0.26f, 0.62f, 0.42f)
                : i % 5 == 0 ? new(0.55f) : new(0.40f);
            float alpha0 = Fade(i, j), alpha1 = Fade(i, j + stride);
            Vector3 startColor = Vector3.Lerp(baseColor, fog, alpha0);
            Vector3 endColor = Vector3.Lerp(baseColor, fog, alpha1);
            AddLine(new((float)x0, (float)y0, 0.04f),
                new((float)x1, (float)y1, 0.04f),
                (float)(step * 0.004d), startColor, endColor);
        }
        return new RenderMesh("editor/metric-grid", vertices.ToArray());

        static float Fade(int i, int j)
        {
            float t = Math.Clamp(Math.Max(Math.Abs(i), Math.Abs(j)) / 64f, 0f, 1f);
            return Math.Clamp((t - 0.65f) / 0.35f, 0f, 1f);
        }

        void AddLine(Vector3 a, Vector3 b, float width, Vector3 ca, Vector3 cb)
        {
            Vector3 halfWidth = Vector3.Normalize(Vector3.Cross(b - a,
                Vector3.UnitZ)) * width;
            Add(a + halfWidth, ca); Add(a - halfWidth, ca); Add(b + halfWidth, cb);
            Add(a - halfWidth, ca); Add(b - halfWidth, cb); Add(b + halfWidth, cb);
        }

        void Add(Vector3 p, Vector3 c) => vertices.AddRange(
            [p.X, p.Y, p.Z, c.X, c.Y, c.Z, 1f]);
    }
}