using System.Numerics;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public bool HasTerrainHud => IsGeoTerrainPreview || HasLocalTerrainContours;

    public GeoTerrainHudPoint? PickTerrainHud(Vector2 pixel, uint width, uint height) =>
        CurrentId == PageId.Terrain && _geoPreview is { } geo
            ? geo.PickHudPoint(pixel, width, height) : null;

    public Vector3? PickLocalTerrainHud(Vector2 pixel, uint width, uint height)
    {
        if (CurrentId != PageId.Terrain || VisibleLocalTerrain() is not { } local) return null;
        return local.Session.TryPick(pixel, width, height, out var hit)
            ? hit.WorldPosition : null;
    }

    public double? TerrainHudMetersPerPixel(uint width, uint height)
    {
        if (CurrentId != PageId.Terrain) return null;
        if (_geoPreview is { } geo) return geo.HudMetersPerPixel(width, height);
        if (width < 4 || height < 4 || VisibleLocalTerrain() is not { } local ||
            local.Session.Scene.Camera is not { } camera) return null;
        var projection = camera.ViewProjection(width, height);
        var target = camera.Target;
        var a = Vector4.Transform(new Vector4(target, 1f), projection);
        var b = Vector4.Transform(new Vector4(target + camera.ViewRight * local.Session.DisplayScale, 1f), projection);
        double pixels = Math.Abs((double)b.X / b.W - (double)a.X / a.W) * width / 2d;
        return double.IsFinite(pixels) && pixels > 0d ? 1d / pixels : null;
    }
}