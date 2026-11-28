using System.Numerics;
using XuanYu.Editor.Preview;
using XuanYu.Render;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private IReadOnlyList<RenderDraw> ComposeEditorDraws(uint width, uint height)
    {
        if (width < 2 || height < 2) return [];
        bool terrainPage = CurrentId == PageId.Terrain;
        var geo = _geoPreview;
        var runtime = geo is null && !terrainPage ? _runtimeBridge : null;
        var preview = geo is not null || runtime is not null ? null : terrainPage
            ? _terrainPreview ??= CreateTerrainPreview()
            : _terrainPreview is { } shared &&
                ReferenceEquals(shared.Scene, Scene)
                ? shared : null;
        var scene = preview?.Scene ?? Scene;
        var camera = runtime is null ? scene.Camera : ActiveCamera;
        var draws = new List<RenderDraw>();

        if (SkyEnabled)
        {
            double pitch = geo?.Camera.PitchRadians ??
                (camera is null ? Math.PI / 4d : Math.Asin(
                    (camera.Position.Z - camera.Target.Z) /
                    Vector3.Distance(camera.Position, camera.Target)));
            draws.Add(ViewportSky.WithPitch(pitch));
        }

        if (geo is not null)
        {
            draws.AddRange(geo.Draws(width, height));
            if ((terrainPage ? TerrainGridEnabled : WorldGridEnabled) &&
                geo.GridDraw(terrainPage, width, height) is { } grid)
                draws.Add(grid);
        }
        else
        {
            Matrix4x4 projection = runtime is null ? scene.ViewProjection(width, height)
                : camera?.ViewProjection(width, height) ?? Matrix4x4.Identity;
            draws.AddRange(runtime is not null ? runtime.Snapshot(projection)
                : preview is null ? Scene.ViewportDraws(width, height)
                : preview.ViewportDraws(width, height));
            bool showGrid = terrainPage ? TerrainGridEnabled : WorldGridEnabled;
            if (showGrid && camera is not null)
                draws.Add(_metricGrid.Draw(camera, projection));
            if (preview is not null && !ReferenceEquals(preview.Scene, Scene))
                draws.AddRange(Scene.RenderWithProjection(projection).Where(
                    draw => !draw.Mesh.AssetId.StartsWith(
                        "terrain/local/", StringComparison.Ordinal)));
        }
        return draws;
    }
}