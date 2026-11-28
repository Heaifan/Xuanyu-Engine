using System.Numerics;
using Avalonia.Controls;
using XuanYu.Core.Geodesy;
using XuanYu.Editor.Camera;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private void DisplayG07HudHover(GeoTerrainRenderSession source, uint width, uint height)
    {
        if (_pendingG07HudPoint is not { } point) return;
        _pendingG07HudPoint = null;
        if (_wb.CurrentId != PageId.Terrain || !_wb.GeoHudVisible ||
            !ReferenceEquals(_wb.G07PreviewSource, source) || _g07Cpu is not { } cpu) return;
        double scaling = TopLevel.GetTopLevel(this)?.RenderScaling ?? 1d;
        var pixel = new Vector2((float)(point.X * scaling), (float)(point.Y * scaling));
        if (!ViewportScreenRay.TryCreate(_g07HudProjection, pixel, width, height,
            out Vector3 start, out Vector3 ray) || !cpu.TryPickSurfaceHit(start, ray, out var hit, _g07HudOrigin))
        {
            _geoHud.ShowPoint(null);
            return;
        }
        var geo = Wgs84.ToGeo(new EnuFrame(source.G07Origin).ToEcef(new EnuPosition(
            _g07HudOrigin.East + hit.Position.X, _g07HudOrigin.North + hit.Position.Y, _g07HudOrigin.Up + hit.Position.Z)));
        int south = (int)Math.Floor(geo.LatitudeDegrees), west = (int)Math.Floor(geo.LongitudeDegrees);
        string tile = $"{(south < 0 ? "S" : "N")}{Math.Abs(south):00}" +
            $"{(west < 0 ? "W" : "E")}{Math.Abs(west):000}";
        _geoHud.ShowPoint(new GeoTerrainHudPoint(geo.LatitudeDegrees,
            geo.LongitudeDegrees, hit.Elevation, tile)
        { LodText = $"LOD {hit.Tile.Level} · 采样 {1 << hit.Tile.Level}″" });
    }
}