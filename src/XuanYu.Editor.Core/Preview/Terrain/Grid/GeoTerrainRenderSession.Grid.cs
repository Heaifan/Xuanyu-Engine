using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Preview;
using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private (bool Geographic, long X, long Y, double Step, long Loads) _gridKey;
    private RenderMesh? _gridMesh;
    private bool _gridInitialized;

    public Matrix4x4 EditorViewProjection(uint width, uint height) =>
        ViewProjection(width, height);

    public void FocusWorldPosition(Vector3 local)
    {
        GeoCoordinate point = Wgs84.ToGeo(_anchor.ToEcef(
            new EnuPosition(local.X, local.Y, local.Z)));
        Camera.Focus(new GeoCoordinate(point.LatitudeDegrees,
            point.LongitudeDegrees, 0d));
    }

    public RenderDraw? GridDraw(bool geographic, uint width, uint height)
    {
        double step = GridStep.FromDistance(Camera.DistanceMeters);
        GeoCoordinate target = Camera.Target;
        double latStep = step / 111_000d;
        double lonStep = latStep /
            Math.Max(0.25d, Math.Cos(target.LatitudeDegrees * Math.PI / 180d));
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
        var key = (geographic, x, y, step, _source.Residency.LoadCount);
        if (!_gridInitialized || _gridKey != key)
        {
            _gridInitialized = true;
            _gridKey = key;
            Func<double, double, double?> sample = Camera.DistanceMeters > 20_000d
                ? _source.CoarseFor(9).Sample : _source.SampleResident;
            _gridMesh = GeoTerrainGridMesh.Create(_anchor,
                sample, target, step, geographic, x, y);
        }
        return _gridMesh is null ? null
            : new RenderDraw(_gridMesh, ViewProjection(width, height), true, false);
    }
}