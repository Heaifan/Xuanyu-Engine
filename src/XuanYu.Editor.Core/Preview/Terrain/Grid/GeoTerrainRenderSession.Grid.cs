using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Render;
using XuanYu.Render.Vulkan;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private (bool Geographic, long X, long Y, double Step, long Loads, GeoUnifiedLodSession? G07, int Half) _gridKey;
    private RenderMesh? _gridMesh;
    private bool _gridInitialized;
    public double? G07GridStepMeters { get; private set; }

    public Matrix4x4 EditorViewProjection(uint width, uint height) =>
        ViewProjection(width, height);

    public void FocusWorldPosition(Vector3 local)
    {
        GeoCoordinate point = Wgs84.ToGeo(_anchor.ToEcef(
            new EnuPosition(local.X, local.Y, local.Z)));
        Camera.Focus(new GeoCoordinate(point.LatitudeDegrees,
            point.LongitudeDegrees, G07EllipsoidHeight(point, 0d)));
    }

    public RenderDraw? GridDraw(bool geographic, uint width, uint height,
        GeoUnifiedLodSession? g07 = null)
    {
        bool unified = g07 is not null;
        double step = GridStep.FromDistance(Camera.DistanceMeters);
        if (unified)
        {
            var levels = GridStep.ForViewport(Camera.DistanceMeters, height);
            step = levels.Blend < 0.5f ? levels.Step : levels.Next;
            G07GridStepMeters = step;
        }
        int half = geographic && unified ? (int)Math.Clamp(Math.Ceiling(
            6d * GridStep.FromDistance(Camera.DistanceMeters) / step), 6d, 32d) : geographic ? 6 : 32;
        var (_, _, lonStep, x, y) = GridPlacement(geographic, unified, step);
        GeoCoordinate target = Camera.Target;
        var key = (geographic, x, y, step, g07 is null ? _source.Residency.LoadCount : 0L, g07, half);
        if (!_gridInitialized || _gridKey != key)
        {
            _gridInitialized = true;
            _gridKey = key;
            Func<double, double, double?> sample = g07 is not null ?
                (lat, lon) => g07.Domain is { } domain && !domain.Contains(lat, lon)
                    ? G07EllipsoidHeight(new GeoCoordinate(lat, lon, 0d), 0d) : null :
                Camera.DistanceMeters > 20_000d ? _source.CoarseFor(9).Sample : _source.SampleResident;
            _gridMesh = GeoTerrainGridMesh.Create(_anchor,
                sample, target, step, geographic, x, y, g07 is null ? null : lonStep, half);
        }
        return _gridMesh is null ? null
            : new RenderDraw(_gridMesh, g07 is null ? ViewProjection(width, height) :
                G07Projection(width, height), true, false);
    }
    public TerrainGridStyle G07GridStyle(bool geographic, uint height)
    {
        var levels = GridStep.ForViewport(Camera.DistanceMeters, height);
        G07GridStepMeters = levels.Blend < 0.5f ? levels.Step : levels.Next;
        var (step, latStep, lonStep, x, y) = GridPlacement(geographic, true, levels.Step);
        return new(geographic, geographic ? lonStep : step,
            geographic ? latStep : step, x, y)
        { NextRatio = step / levels.Next, Blend = levels.Blend };
    }

    private (double Step, double LatStep, double LonStep, long X, long Y) GridPlacement(
        bool geographic, bool unified, double step)
    {
        GeoCoordinate target = Camera.Target;
        double latStep = step / 111_000d;
        double lonStep = latStep /
            Math.Max(0.25d, Math.Cos((unified ? _anchor.Origin.LatitudeDegrees :
                target.LatitudeDegrees) * Math.PI / 180d));
        EnuPosition local = _anchor.ToEnu(Camera.TargetEcef);
        long x = geographic
            ? (long)Math.Round(target.LongitudeDegrees / lonStep)
            : (long)Math.Round(local.East / step);
        long y = geographic
            ? (long)Math.Round(target.LatitudeDegrees / latStep)
            : (long)Math.Round(local.North / step);
        if (!geographic)
        {
            x = (long)Math.Round(x / 8d) * 8;
            y = (long)Math.Round(y / 8d) * 8;
        }
        return (step, latStep, lonStep, x, y);
    }
}