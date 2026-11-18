using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private double? SampleGroundHeight(GeoCoordinate position)
    {
        double? height = _source.Sample(
            position.LatitudeDegrees, position.LongitudeDegrees);
        if (_visibleMeshes.Count == 0) return height ?? 0d;

        EnuPosition eye = _anchor.ToEnu(Wgs84.ToEcef(position));
        foreach (var (id, mesh) in _visibleMeshes)
        {
            GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
            if (position.LatitudeDegrees < bounds.SouthLatitudeDegrees ||
                position.LatitudeDegrees > bounds.NorthLatitudeDegrees ||
                position.LongitudeDegrees < bounds.WestLongitudeDegrees ||
                position.LongitudeDegrees > bounds.EastLongitudeDegrees)
                continue;
            double? z = GeoTerrainMeshSurface.HeightAt(mesh, eye.East, eye.North);
            if (z is null) continue;
            EcefPosition terrain = _anchor.ToEcef(
                new EnuPosition(eye.East, eye.North, z.Value));
            double altitude = Wgs84.ToGeo(terrain).HeightMeters;
            height = height is { } old ? Math.Max(old, altitude) : altitude;
        }
        // 未加载 HGT 的区域仍须受椭球零高程保护，不能无限钻入地球内部。
        return height ?? 0d;
    }
}