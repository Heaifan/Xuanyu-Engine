using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private double? SampleGroundHeight(GeoCoordinate position)
    {
        // 保留 G04 眼点精度合同：极高海拔无须触发 HGT 同步读取。
        double? raw = StreamingEnabled && position.HeightMeters > short.MaxValue + 3d
            ? null : _source.Sample(position.LatitudeDegrees, position.LongitudeDegrees);
        if (_g07Geoid is not null) return G07EllipsoidHeight(position, raw ?? 0d);
        return SampleDisplayHeight(position, raw);
    }

    private double? SampleFocusHeight(GeoCoordinate position)
    {
        // G05 流式模式仅用已载入概览 + 可见网格，禁止焦点同步解码原始 HGT。
        var sampler = StreamingEnabled ? _source.CoarseFor(13) : _source;
        double? height = sampler.Sample(position.LatitudeDegrees,
            position.LongitudeDegrees);
        if (_g07Geoid is not null) return G07EllipsoidHeight(position, height ?? 0d);
        return SampleDisplayHeight(position, height);
    }

    private double? SampleDisplayHeight(GeoCoordinate position, double? height)
    {
        if (_visibleMeshes.Count == 0) return height ?? 0d;
        // 地表网格的 XY 是以零椭球高转换的，查询也必须使用同一平面坐标。
        var zero = new GeoCoordinate(position.LatitudeDegrees,
            position.LongitudeDegrees, 0d);
        EnuPosition planar = _anchor.ToEnu(Wgs84.ToEcef(zero));
        foreach (var (id, mesh) in _visibleMeshes)
        {
            GeoTileBounds bounds = GeographicTileScheme.Bounds(id);
            if (position.LatitudeDegrees < bounds.SouthLatitudeDegrees ||
                position.LatitudeDegrees > bounds.NorthLatitudeDegrees ||
                position.LongitudeDegrees < bounds.WestLongitudeDegrees ||
                position.LongitudeDegrees > bounds.EastLongitudeDegrees)
                continue;
            double? z = GeoTerrainMeshSurface.HeightAt(mesh, planar.East, planar.North);
            if (z is null) continue;
            EcefPosition terrain = _anchor.ToEcef(
                new EnuPosition(planar.East, planar.North, z.Value));
            double altitude = Wgs84.ToGeo(terrain).HeightMeters;
            height = height is { } old ? Math.Max(old, altitude) : altitude;
        }
        return height ?? 0d;
    }
}