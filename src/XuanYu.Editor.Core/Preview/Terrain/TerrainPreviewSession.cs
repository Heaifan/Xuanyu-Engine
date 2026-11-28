using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.Render;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class TerrainPreviewSession
{
    public MapDocument Map { get; }
    public PreviewScene Scene { get; }
    public TerrainMeshGeometry? Geometry { get; private set; }
    public float DisplayScale { get; }
    public Vector2 OriginMeters { get; }
    public bool IsCentered { get; }
    public Guid? TerrainObjectId { get; }

    public TerrainPreviewSession() : this(TerrainPreviewMap.Create()) { }

    public TerrainPreviewSession(MapDocument map,
        PreviewScene? sharedScene = null, Guid? replacingId = null,
        bool centered = false, Guid? existingId = null)
    {
        Map = map;
        IsCentered = centered;
        OriginMeters = centered
            ? new Vector2(map.WidthMeters / 2f, map.LengthMeters / 2f)
            : Vector2.Zero;
        DisplayScale = MathF.Min(1f,
            256f / MathF.Max(map.WidthMeters, map.LengthMeters));
        Scene = sharedScene ?? new PreviewScene();
        var camera = Scene.Camera ?? new ViewportCamera();
        Scene.Camera = camera;
        if (replacingId is Guid id) Scene.Delete(id);

        if (map.HeightField.SamplesX > 257 || map.HeightField.SamplesY > 257)
            CreateChunks();
        else
        {
            Geometry = TerrainMeshBuilder.Build(map.HeightField);
            if (existingId is Guid loadedId) TerrainObjectId = loadedId;
            else
            {
                RenderMesh mesh = centered
                    ? TerrainRenderMeshAdapter.CreateLocal($"terrain/local/{Guid.NewGuid():N}", Geometry)
                    : TerrainRenderMeshAdapter.Create(Geometry);
                Matrix4x4 model = Matrix4x4.CreateScale(DisplayScale) *
                    Matrix4x4.CreateTranslation(
                        -OriginMeters.X * DisplayScale,
                        -OriginMeters.Y * DisplayScale, 0f);
                TerrainObjectId = Scene.Add(map.Name, mesh, model).Id;
            }
        }
        if (centered && TerrainObjectId is Guid terrainId)
            Scene.RegisterLocalTerrain(map, terrainId);

        TerrainHeightField field = map.HeightField;
        float z = centered ? 0f : DisplayScale == 1f ? 12f :
            field.GetHeight(field.SamplesX / 2, field.SamplesY / 2)
                * DisplayScale;
        var center = new Vector3(
            (map.WidthMeters / 2f - OriginMeters.X) * DisplayScale,
            (map.LengthMeters / 2f - OriginMeters.Y) * DisplayScale, z);
        float radius = centered
            ? MathF.Max(map.WidthMeters, map.LengthMeters) * DisplayScale * 0.7f
            : 170f;
        camera.Frame(center, radius, 1.4f);
        camera.SetTerrainCollision(SampleSurface);
        RefreshLocalCameraBounds();
    }

    public IReadOnlyList<RenderDraw> ViewportDraws(uint width, uint height)
    {
        UpdateChunks();
        IReadOnlyList<RenderDraw> draws = Scene.ViewportDraws(width, height);
        if (_chunks.Count == 0 || Scene.SelectedId is null) return draws;
        return draws.Select(draw => draw with { Selected = true }).ToArray();
    }

    public bool TryPick(Vector2 pixel, uint width, uint height,
        out TerrainRayHit hit, TerrainHeightField? reference = null)
    {
        hit = default;
        if (Scene.Camera is not { } camera) return false;
        if (!camera.TryCreateScreenRay(pixel, width, height,
            out Vector3 origin, out Vector3 direction))
            return false;
        Vector3 dataOrigin = origin / DisplayScale +
            new Vector3(OriginMeters, 0f);
        return TerrainHeightRayPicker.TryPick(
            reference ?? Map.HeightField, dataOrigin, direction, out hit);
    }
}