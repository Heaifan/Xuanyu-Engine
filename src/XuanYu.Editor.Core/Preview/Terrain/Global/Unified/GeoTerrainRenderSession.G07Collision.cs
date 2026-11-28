using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private double CorrectedG07CollisionHeight(GeoUnifiedLodSession cpu,
        GeoCoordinate position, double? raw)
    {
        double ground = G07EllipsoidHeight(position, raw ?? 0d);
        double topHeight = Math.Max(position.HeightMeters + 1d, short.MaxValue + 201d);
        var top = _anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(
            position.LatitudeDegrees, position.LongitudeDegrees, topHeight)));
        var zero = _anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(
            position.LatitudeDegrees, position.LongitudeDegrees, 0d)));
        var direction = Vector3.Normalize(new Vector3((float)(zero.East - top.East),
            (float)(zero.North - top.North), (float)(zero.Up - top.Up)));
        if (!cpu.TryPickSurfaceHit(Vector3.Zero, direction, out var hit, top)) return ground;
        double rendered = Wgs84.ToGeo(_anchor.ToEcef(new EnuPosition(
            top.East + hit.Position.X, top.North + hit.Position.Y, top.Up + hit.Position.Z))).HeightMeters;
        return Math.Max(ground, rendered);
    }
}