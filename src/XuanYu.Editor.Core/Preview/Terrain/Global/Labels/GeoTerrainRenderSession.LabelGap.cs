using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private readonly GeoContourLabelVisibility _labelVisibility = new();
    public GeoContourLabel[] BindContourLabelMeshes(
        IReadOnlyList<RenderDraw> draws, IReadOnlyList<GeoContourLabel> anchors,
        uint width, uint height)
    {
        if (!ContoursEnabled || !ContourLabelsEnabled) return [];
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
                if (draw.ResidentDem is not { } dem || !GeoContourLabelDem.TryTangent(dem, uv, out Vector2 direction)) continue;
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
        return result.ToArray();
    }

    public RenderDraw[] AttachContourLabelGaps(IReadOnlyList<RenderDraw> draws,
        IReadOnlyList<GeoContourLabel> anchors, uint width, uint height, float scale,
        out GeoContourLabel[] visible, Func<int, float>? measure = null)
    {
        RenderDraw[] result = draws.ToArray();
        var labels = new List<GeoContourLabel>();
        if (!ContoursEnabled || !ContourLabelsEnabled || anchors.Count == 0)
        {
            visible = [];
            return result;
        }
        Matrix4x4 projection = ViewProjection(width, height);
        _labelVisibility.Prepare(result, projection);
        foreach (GeoContourLabel anchor in anchors)
        {
            int index = Array.FindIndex(result, d =>
                ReferenceEquals(d.Mesh, anchor.Owner) && CanOwn(d, anchor));
            if (index < 0 || result[index].ContourLabelGap != Vector4.Zero ||
                labels.Any(old => old.ElevationMeters == anchor.ElevationMeters) ||
                !GeoContourLabelProjection.TryScreen(anchor,
                projection, width, height, out GeoContourLabel label)) continue;
            if (labels.Any(old => MathF.Abs(old.X - label.X) < 72f &&
                                  MathF.Abs(old.Y - label.Y) < 24f)) continue;
            float radians = label.AngleDegrees * MathF.PI / 180f;
            float halfWidth = ((measure?.Invoke(label.ElevationMeters) ?? 64f) / 2f + 2f) * scale;
            if (!_labelVisibility.IsVisible(label, halfWidth, radians, width, height)) continue;
            uint packed = (uint)BitConverter.HalfToUInt16Bits((Half)radians) |
                ((uint)BitConverter.HalfToUInt16Bits((Half)halfWidth) << 16);
            result[index] = result[index] with
            {
                ContourLabelGap = new Vector4(label.X, label.Y,
                    BitConverter.UInt32BitsToSingle(packed), label.ElevationMeters)
            };
            labels.Add(label);
            if (labels.Count == 7) break;
        }
        visible = labels.ToArray();
        return result;
    }

    private static bool CanOwn(RenderDraw draw, GeoContourLabel label) =>
        draw.Visible && !draw.LegacyContourOnly &&
        draw.ContourIntervalMeters > 0f && draw.ContourOpacity > 0f &&
        draw.ResidentDem is { } dem && !draw.DemUV.IsEmpty &&
        dem.SouthLatitude == label.South && dem.WestLongitude == label.West &&
        label.ElevationMeters % (int)draw.ContourIntervalMeters == 0;
}