using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public bool StreamingEnabled { get; private set; }
    public long BackgroundLoadCount => _source.BackgroundLoadCount;
    public bool RawPrefetchPending => _source.RawPrefetchPending;

    private void PrefetchCameraSources()
    {
        if (!StreamingEnabled) return;
        GeoCoordinate eye = Wgs84.ToGeo(Camera.PositionEcef);
        if (Camera.DistanceMeters > 20_000d && eye.HeightMeters > short.MaxValue + 3d) return;
        RequestCoordinate(eye);
        RequestCoordinate(Camera.Target);
    }

    private void RequestCoordinate(GeoCoordinate point)
    {
        int south = Math.Min(89, (int)Math.Floor(point.LatitudeDegrees));
        int west = point.LongitudeDegrees == 180d ? -180 : (int)Math.Floor(point.LongitudeDegrees);
        _source.PrefetchRaw(south, west);
    }
}