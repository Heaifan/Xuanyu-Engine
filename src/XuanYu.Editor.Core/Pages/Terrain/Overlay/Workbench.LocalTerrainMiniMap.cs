using System.Numerics;
using XuanYu.Editor.Preview.Terrain;
using XuanYu.Render;

namespace XuanYu.Editor.Pages;

public sealed partial class Workbench
{
    private RenderMesh? _localMiniMapMesh;
    private GeoTerrainMiniMapData? _localMiniMap;

    private (TerrainPreviewSession Session, RenderMesh Mesh)? VisibleLocalTerrain()
    {
        if (!HasLocalTerrainContours) return null;
        var session = _terrainPreview!;
        var item = session.Scene.Objects.FirstOrDefault(o => o.Id == session.TerrainObjectId);
        return item is { Visible: true } ? (session, item.Mesh) : null;
    }

    private GeoTerrainMiniMapData? LocalTerrainMiniMap()
    {
        if (VisibleLocalTerrain() is not { } local) return null;
        if (ReferenceEquals(_localMiniMapMesh, local.Mesh)) return _localMiniMap;
        const int columns = 48, rows = 64;
        var field = local.Session.Map.HeightField;
        var cells = new byte[columns * rows];
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < columns; x++)
        {
            int sx = (int)Math.Round((x + 0.5d) * (field.SamplesX - 1) / columns);
            int sy = (int)Math.Round((1d - (y + 0.5d) / rows) * (field.SamplesY - 1));
            cells[y * columns + x] = GeoTerrainMiniMapData.HeightBand(field.GetHeight(sx, sy));
        }
        _localMiniMapMesh = local.Mesh;
        return _localMiniMap = new(0d, 0d, field.LengthMeters, field.WidthMeters,
            columns, rows, cells) { IsLocal = true };
    }

    public (double North, double East, double Yaw)? TerrainMiniMapCamera()
    {
        if (CurrentId != PageId.Terrain) return null;
        if (GeoTerrainCamera is { } geo)
            return (geo.Target.LatitudeDegrees, geo.Target.LongitudeDegrees, geo.YawRadians);
        if (VisibleLocalTerrain() is not { } local || local.Session.Scene.Camera is not { } camera)
            return null;
        Vector3 target = camera.Target / local.Session.DisplayScale +
            new Vector3(local.Session.OriginMeters, 0f);
        Vector3 right = camera.ViewRight;
        return (target.Y, target.X, Math.Atan2(right.Y, right.X));
    }
}