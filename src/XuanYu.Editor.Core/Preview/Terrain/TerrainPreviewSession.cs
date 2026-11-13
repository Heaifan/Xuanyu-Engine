using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed class TerrainPreviewSession
{
    public MapDocument Map { get; }
    public PreviewScene Scene { get; }
    public TerrainMeshGeometry Geometry { get; }
    public float DisplayScale { get; }

    public TerrainPreviewSession() : this(TerrainPreviewMap.Create()) { }

    public TerrainPreviewSession(MapDocument map)
    {
        Map = map;
        DisplayScale = MathF.Min(1f,
            256f / MathF.Max(map.WidthMeters, map.LengthMeters));
        var camera = new ViewportCamera();
        Scene = new PreviewScene { Camera = camera };
        Geometry = TerrainMeshBuilder.Build(map.HeightField);
        Scene.Add(map.Name, TerrainRenderMeshAdapter.Create(Geometry),
            Matrix4x4.CreateScale(DisplayScale));

        float z = DisplayScale == 1f ? 12f :
            map.HeightField.GetHeight(map.HeightField.SamplesX / 2,
                map.HeightField.SamplesY / 2) * DisplayScale;
        Vector3 center = new(map.WidthMeters * DisplayScale * 0.5f,
            map.LengthMeters * DisplayScale * 0.5f, z);
        camera.Frame(center, 170f, 1.4f);
    }

    public bool TryPick(Vector2 pixel, uint width, uint height,
        out TerrainRayHit hit)
    {
        hit = default;
        if (width == 0 || height == 0 || Scene.Camera is not { } camera)
            return false;
        if (!ViewportScreenRay.TryCreate(
            camera.ViewProjection(width, height), pixel, width, height,
            out Vector3 origin, out Vector3 direction))
            return false;
        return TerrainRayPicker.TryPick(Geometry,
            origin / DisplayScale, direction, out hit);
    }
}