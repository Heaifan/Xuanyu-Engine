
using System.Numerics;
using XuanYu.Editor.Camera;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed class TerrainPreviewSession
{
    public MapDocument Map { get; }
    public PreviewScene Scene { get; }

    public TerrainPreviewSession()
    {
        Map = TerrainPreviewMap.Create();
        Scene = new PreviewScene
        {
            Camera = new ViewportCamera()
        };

        TerrainMeshGeometry geometry =
            TerrainMeshBuilder.Build(Map.HeightField);

        Scene.Add(
            "Terrain_E4",
            TerrainRenderMeshAdapter.Create(geometry),
            Matrix4x4.Identity);

        Vector3 center = new(
            Map.WidthMeters * 0.5f,
            Map.LengthMeters * 0.5f,
            12f);

        Scene.Camera.Frame(center, 170f, 1.4f);
    }
}
