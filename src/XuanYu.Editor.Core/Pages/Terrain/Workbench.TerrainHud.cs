using System.Numerics;
using XuanYu.Editor.Preview.Terrain;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    public GeoTerrainHudPoint? PickTerrainHud(Vector2 pixel, uint width, uint height) =>
        CurrentId == PageId.Terrain && _geoPreview is { } geo
            ? geo.PickHudPoint(pixel, width, height) : null;

    public double? TerrainHudMetersPerPixel(uint width, uint height) =>
        CurrentId == PageId.Terrain && _geoPreview is { } geo
            ? geo.HudMetersPerPixel(width, height) : null;
}