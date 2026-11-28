using System.Numerics;
using XuanYu.Render;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.Runtime;

public sealed class WorldRenderBridge(WorldScene world)
{
    private readonly Dictionary<Guid, RenderMesh> _meshes = [];

    public void Bind(Guid id, RenderMesh mesh)
    {
        if (world.Find(id) is null)
            throw new KeyNotFoundException($"World 实体不存在：{id}");

        _meshes[id] = mesh;
    }

    public bool Unbind(Guid id) => _meshes.Remove(id);

    public RenderDraw[] Snapshot(
        Matrix4x4 worldToClip,
        Guid? selectedId = null)
    {
        List<RenderDraw> draws = [];

        foreach (WorldEntity entity in world.Entities)
        {
            if (entity.Transform.Spatial.Space != SpatialSpace.Local ||
                !_meshes.TryGetValue(entity.Id, out RenderMesh? mesh))
                continue;

            draws.Add(new RenderDraw(
                mesh,
                entity.Transform.ToMatrix() * worldToClip,
                entity.IsVisible,
                entity.Id == selectedId));
        }

        return [.. draws];
    }
    public RenderDraw[] SnapshotGeographic(GeoRenderFrame frame, Guid? selectedId = null)
    {
        List<RenderDraw> draws = [];
        foreach (WorldEntity entity in world.Entities)
        {
            if (entity.Transform.Spatial.Space != SpatialSpace.Ecef ||
                !_meshes.TryGetValue(entity.Id, out RenderMesh? mesh)) continue;
            draws.Add(new RenderDraw(mesh, frame.ModelToClip(entity.Transform),
                entity.IsVisible, entity.Id == selectedId));
        }
        return [.. draws];
    }
}