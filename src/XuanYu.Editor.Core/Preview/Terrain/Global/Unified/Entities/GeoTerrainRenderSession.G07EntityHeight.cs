using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public (double Orthometric, double? AboveGround) G07EntityHeights(
        GeoUnifiedLodSession cpu, GeoCoordinate position)
    {
        var geoid = _g07Geoid ?? throw new InvalidOperationException("G07 尚未配置 EGM96 格网。");
        double height = position.HeightMeters - geoid.UndulationMeters(position);
        var ground = cpu.SampleElevation(position);
        return (height, ground.HeightMeters is double h ? height - h : null);
    }

    public SpatialPosition ResolveG07Orthometric(GeoCoordinate position)
    {
        var geoid = _g07Geoid ?? throw new InvalidOperationException("G07 尚未配置 EGM96 格网。");
        return SpatialPosition.FromGeo(new GeoCoordinate(position.LatitudeDegrees,
            position.LongitudeDegrees, position.HeightMeters + geoid.UndulationMeters(position)));
    }
}