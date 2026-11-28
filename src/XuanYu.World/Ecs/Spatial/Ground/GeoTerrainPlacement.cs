using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.World.Ecs;

public sealed class GeoTerrainPlacement(GeoUnifiedLodSession terrain, Egm96GeoidGrid geoid)
{
    public GeoElevationSample ResolveGround(GeoCoordinate horizontal, double offsetMeters,
        out SpatialPosition position)
    {
        if (!double.IsFinite(offsetMeters) || offsetMeters < 0d)
            throw new ArgumentOutOfRangeException(nameof(offsetMeters));
        position = default;
        GeoElevationSample sample = terrain.SampleElevation(horizontal);
        if (sample.Status != GeoSampleStatus.Valid) return sample;
        double height = sample.HeightMeters!.Value + geoid.UndulationMeters(horizontal)
            + offsetMeters;
        position = SpatialPosition.FromGeo(new GeoCoordinate(horizontal.LatitudeDegrees,
            horizontal.LongitudeDegrees, height));
        return sample;
    }

    public GeoElevationSample PlaceOnGround(WorldScene world, Guid id,
        double latitudeDegrees, double longitudeDegrees, double offsetMeters = 0d)
    {
        var horizontal = new GeoCoordinate(latitudeDegrees, longitudeDegrees, 0d);
        if (world.GetSpatialPosition(id).Space != SpatialSpace.Ecef)
            throw new InvalidOperationException("贴地定位仅适用于已建立地理参考的全球实体。");
        var sample = ResolveGround(horizontal, offsetMeters, out var position);
        if (sample.Status == GeoSampleStatus.Valid) world.SetSpatialPosition(id, position);
        return sample;
    }
}