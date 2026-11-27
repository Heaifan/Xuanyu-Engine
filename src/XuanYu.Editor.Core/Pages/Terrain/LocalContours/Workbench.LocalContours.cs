using System.Numerics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private bool _localContoursEnabled = true;
    public bool HasLocalTerrainContours => _geoPreview is null &&
        _terrainPreview is { IsCentered: true, TerrainObjectId: not null };

    private static GeoContourStyle GetLocalContourStyle(TerrainPreviewSession session)
    {
        var camera = session.Scene.Camera!;
        double distanceMeters = Vector3.Distance(camera.Position, camera.Target) /
            session.DisplayScale;
        return GeoContourStyle.AtLocalDistance(distanceMeters);
    }

    public IReadOnlyList<RenderDraw> ApplyLocalTerrainContours(IReadOnlyList<RenderDraw> draws)
    {
        if ((IsSimulating && CurrentId != PageId.Terrain) || !HasLocalTerrainContours) return draws;
        var session = _terrainPreview!;
        var item = session.Scene.Objects.FirstOrDefault(o => o.Id == session.TerrainObjectId);
        if (item is null || !item.Visible) return draws;
        GeoContourStyle style = GetLocalContourStyle(session);
        var result = draws.ToArray();
        for (int i = 0; i < result.Length; i++)
        {
            var draw = result[i];
            if (!draw.Visible || !ReferenceEquals(draw.Mesh, item.Mesh)) continue;
            result[i] = draw with
            {
                ContourIntervalMeters = _localContoursEnabled ? style.IntervalMeters : 0,
                ContourNextIntervalMeters = _localContoursEnabled ? style.NextIntervalMeters : 0,
                ContourIntervalBlend = _localContoursEnabled ? style.IntervalBlend : 0f,
                ContourOpacity = _localContoursEnabled ? style.Opacity : 0f
            };
        }
        return result;
    }
}