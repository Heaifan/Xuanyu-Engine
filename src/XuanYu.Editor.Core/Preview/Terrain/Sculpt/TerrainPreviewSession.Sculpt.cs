using System.Numerics;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    internal void RefreshSculptMesh()
    {
        Guid id = TerrainObjectId!.Value;
        var item = Scene.Objects.First(obj => obj.Id == id);
        var geometry = Geometry!;
        var mesh = LocalTerrainMeshUpdate.Apply(Map.HeightField, geometry, item.Mesh);
        if (mesh is null) return;
        Scene.ReplaceMesh(id, mesh);
        RefreshLocalCameraBounds();
    }

    private void RefreshLocalCameraBounds()
    {
        if (!IsCentered || Scene.Camera is not { } camera) return;
        var field = Map.HeightField;
        float min = float.PositiveInfinity, max = float.NegativeInfinity;
        foreach (float value in field.Samples)
        {
            min = MathF.Min(min, value);
            max = MathF.Max(max, value);
        }
        var lower = new Vector3(-OriginMeters, min) * DisplayScale;
        var upper = new Vector3(field.WidthMeters - OriginMeters.X,
            field.LengthMeters - OriginMeters.Y, max) * DisplayScale;
        camera.SetLocalTerrainBounds(lower, upper);
        camera.LiftAboveTerrain();
    }
}