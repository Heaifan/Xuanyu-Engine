using System.Numerics;
using Xunit;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.World.Tests.Maps;

internal static class G06GeometryMeasure
{
    public static List<Vector3> Samples(NasademTile raw, EnuFrame anchor, GeoCoordinate focus)
    {
        int col = (int)Math.Round((focus.LongitudeDegrees - raw.WestLongitude) * 3600);
        int row = (int)Math.Round((focus.LatitudeDegrees - raw.SouthLatitude) * 3600);
        var samples = new List<Vector3>();
        // 固定中心 17×17 原始采样点；避免把 Native 共边过渡混入内部精度基线。
        for (int y = row - 24; y <= row + 24; y += 3)
            for (int x = col - 24; x <= col + 24; x += 3)
            {
                if (raw.HeightAt(x, y) is not { } height) continue;
                var geo = new GeoCoordinate(raw.SouthLatitude + y / 3600d,
                    raw.WestLongitude + x / 3600d, 0);
                EnuPosition p = anchor.ToEnu(Wgs84.ToEcef(geo));
                samples.Add(new((float)p.East, (float)p.North, (float)(p.Up + height)));
            }
        Assert.True(samples.Count > 0, "原始 HGT 无有效采样。");
        return samples;
    }

    public static string Report(string label, RenderDraw draw, Matrix4x4 common,
        IReadOnlyList<Vector3> samples)
    {
        var heights = new List<double>();
        var pixels = new List<double>();
        var actual = new List<double>();
        int miss = 0;
        foreach (Vector3 reference in samples)
        {
            double? z = GeoTerrainMeshSurface.HeightAt(draw.Mesh, reference.X, reference.Y);
            if (z is not { } surface) { miss++; continue; }
            heights.Add(Math.Abs(surface - reference.Z));
            var point = new Vector3(reference.X, reference.Y, (float)surface);
            AddProjection(pixels, point, reference, common);
            AddProjection(actual, point, reference, draw.ModelToClip);
        }
        Assert.True(heights.Count > 0, $"{label} 没有实际网格命中。");
        return $"[G06/R04A] {label} mesh={draw.Mesh.AssetId} 顶点={draw.Mesh.VertexCount} " +
            $"有效HGT={samples.Count}/289 网格命中={heights.Count} 缺面={miss} " +
            $"高程(m) {Stats(heights)} 统一近景(px) {Stats(pixels)} 实际机位(px) {Stats(actual)}";
    }

    private static void AddProjection(List<double> values, Vector3 point, Vector3 reference, Matrix4x4 matrix)
    {
        if (TryPixel(point, matrix, out var a) && TryPixel(reference, matrix, out var b))
            values.Add(Vector2.Distance(a, b));
    }

    private static bool TryPixel(Vector3 p, Matrix4x4 matrix, out Vector2 screen)
    {
        Vector4 clip = Vector4.Transform(new Vector4(p, 1), matrix);
        screen = default;
        if (clip.W <= 0) return false;
        var ndc = new Vector3(clip.X, clip.Y, clip.Z) / clip.W;
        if (Math.Abs(ndc.X) > 1 || Math.Abs(ndc.Y) > 1 || ndc.Z < 0 || ndc.Z > 1) return false;
        screen = new((ndc.X + 1) * 640, (ndc.Y + 1) * 360);
        return true;
    }

    private static string Stats(List<double> errors)
    {
        if (errors.Count == 0) return "N=0";
        errors.Sort();
        double rmse = Math.Sqrt(errors.Sum(e => e * e) / errors.Count);
        double p95 = errors[(int)Math.Ceiling(errors.Count * 0.95) - 1];
        return FormattableString.Invariant($"N={errors.Count} RMSE={rmse:F3} P95={p95:F3} Max={errors[^1]:F3}");
    }
}