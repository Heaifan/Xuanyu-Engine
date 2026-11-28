using XuanYu.Core.Geodesy;
using XuanYu.World.Ecs;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    public GeoElevationSample ResolveG07Ground(GeoUnifiedLodSession cpu,
        GeoCoordinate horizontal, double offsetMeters, out SpatialPosition position)
    {
        var geoid = _g07Geoid ?? throw new InvalidOperationException("G07 尚未配置 EGM96 格网。");
        return new GeoTerrainPlacement(cpu, geoid).ResolveGround(horizontal, offsetMeters, out position);
    }
}