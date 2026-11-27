using System.Diagnostics;
using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoContourLabelVisibility _labelVisibility = new();
    private long _lastLabelBindReport = -60;

    public GeoContourLabel[] BindContourLabelMeshes(
        IReadOnlyList<RenderDraw> draws, IReadOnlyList<GeoContourLabel> anchors,
        uint width, uint height)
    {
        if (!ContoursEnabled || !ContourLabelsEnabled) return [];
        long start = _r02PerfEnabled ? Stopwatch.GetTimestamp() : 0;
        Matrix4x4 projection = ViewProjection(width, height);
        var result = new List<GeoContourLabel>();
        foreach (GeoContourLabel label in anchors)
        {
            foreach (RenderDraw draw in draws)
            {
                if (!CanOwn(draw, label)) continue;
                GeoCoordinate geo = Wgs84.ToGeo(_anchor.ToEcef(new EnuPosition(
                    label.East, label.North, label.Up)));
                if (GeoContourLabelWater.NearWater(draw.WaterAtlas, geo)) continue;
                var uv = new Vector2((float)(geo.LongitudeDegrees - label.West),
                    (float)(label.South + 1d - geo.LatitudeDegrees));
                if (draw.ResidentDem is not { } dem ||
                    !GeoContourLabelDem.TryTangent(dem, uv, out Vector2 direction)) continue;
                if (!GeoContourLabelSurface.TryLocate(draw, uv, direction,
                    out Vector3 surface, out Vector3 tangent)) continue;
                var bound = label with { East = surface.X, North = surface.Y,
                    Up = surface.Z, TangentEast = tangent.X, TangentNorth = tangent.Y,
                    TangentUp = tangent.Z, Owner = draw.Mesh };
                if (!GeoContourLabelProjection.TryScreen(bound, projection,
                    width, height, out _)) continue;
                result.Add(bound);
                break;
            }
        }
        if (_r02PerfEnabled && _monitorFrame - _lastLabelBindReport >= 60)
        {
            _lastLabelBindReport = _monitorFrame;
            Notice?.Invoke($"[G06/R08-03B/标高绑定] 帧={_monitorFrame} 候选={anchors.Count} 绑定={result.Count} 耗时={Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1}ms");
        }
        return result.ToArray();
    }

    private static bool CanOwn(RenderDraw draw, GeoContourLabel label) =>
        draw.Visible && !draw.LegacyContourOnly &&
        draw.ContourIntervalMeters > 0f && draw.ContourOpacity > 0f &&
        draw.ResidentDem is { } dem && !draw.DemUV.IsEmpty &&
        dem.SouthLatitude == label.South && dem.WestLongitude == label.West &&
        label.ElevationMeters % (int)draw.ContourIntervalMeters == 0;
}