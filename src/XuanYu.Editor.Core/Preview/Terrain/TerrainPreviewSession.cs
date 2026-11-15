using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    public MapDocument Map { get; }
    public PreviewScene Scene { get; }
    public TerrainMeshGeometry? Geometry { get; }
    public float DisplayScale { get; }

    public TerrainPreviewSession() : this(TerrainPreviewMap.Create()) { }

    public TerrainPreviewSession(MapDocument map)
    {
        Map = map;
        DisplayScale = MathF.Min(1f,
            256f / MathF.Max(map.WidthMeters, map.LengthMeters));
        var camera = new ViewportCamera();
        Scene = new PreviewScene { Camera = camera };
        if (map.HeightField.SamplesX > 257 || map.HeightField.SamplesY > 257)
            CreateChunks();
        else
        {
            Geometry = TerrainMeshBuilder.Build(map.HeightField);
            Scene.Add(map.Name, TerrainRenderMeshAdapter.Create(Geometry),
                Matrix4x4.CreateScale(DisplayScale));
        }

        TerrainHeightField field = map.HeightField;
        float z = DisplayScale == 1f ? 12f :
            field.GetHeight(field.SamplesX / 2, field.SamplesY / 2) * DisplayScale;
        var center = new Vector3(map.WidthMeters * DisplayScale * 0.5f,
            map.LengthMeters * DisplayScale * 0.5f, z);
        camera.Frame(center, 170f, 1.4f);
        camera.SetTerrainCollision(SampleSurface);
    }

    public IReadOnlyList<RenderDraw> ViewportDraws(uint width, uint height)
    {
        UpdateChunks();
        IReadOnlyList<RenderDraw> draws = Scene.ViewportDraws(width, height);
        if (_chunks.Count == 0 || Scene.SelectedId is null) return draws;
        return draws.Select(draw => draw with { Selected = true }).ToArray();
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
        return TerrainHeightRayPicker.TryPick(Map.HeightField,
            origin / DisplayScale, direction, out hit);
    }
}