using System.Numerics;
using XuanYu.Render;
using XuanYu.World;
using XuanYu.World.Ecs;

namespace XuanYu.Runtime;

public sealed class WorldRenderBridge(WorldScene world)
{
    private readonly Dictionary<Guid, EntityRenderBinding> _bindings = [];

    public void Bind(Guid id, RenderMesh mesh) =>
        BindRepresentation(id, EntityRepresentationKind.Mesh, mesh);

    public void BindRepresentation(Guid id, EntityRepresentationKind kind, RenderMesh mesh)
    {
        RequireEntity(id);
        ArgumentNullException.ThrowIfNull(mesh);
        if (kind is not (EntityRepresentationKind.Mesh or EntityRepresentationKind.Proxy))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (!_bindings.TryGetValue(id, out var binding))
            binding = new(null, null, kind);
        _bindings[id] = kind == EntityRepresentationKind.Mesh
            ? binding with { Mesh = mesh } : binding with { Proxy = mesh };
    }

    public EntityRepresentationKind GetRepresentation(Guid id)
    {
        RequireEntity(id);
        return _bindings.TryGetValue(id, out var binding) ? binding.Active : EntityRepresentationKind.None;
    }

    public void SetRepresentation(Guid id, EntityRepresentationKind kind)
    {
        RequireEntity(id);
        if (kind is not (EntityRepresentationKind.None or EntityRepresentationKind.Mesh or EntityRepresentationKind.Proxy))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (!_bindings.TryGetValue(id, out var binding))
        {
            if (kind == EntityRepresentationKind.None) return;
            throw new InvalidOperationException("实体尚未绑定表现资源。");
        }
        if (kind != EntityRepresentationKind.None && binding.Resource(kind) is null)
            throw new InvalidOperationException("目标表现资源尚未绑定。");
        _bindings[id] = binding with { Active = kind };
    }

    public bool Unbind(Guid id) => _bindings.Remove(id);

    public RenderDraw[] Snapshot(Matrix4x4 worldToClip, Guid? selectedId = null)
    {
        List<RenderDraw> draws = [];
        foreach (WorldEntity entity in world.Entities)
        {
            if (entity.Transform.Spatial.Space != SpatialSpace.Local ||
                GetActiveMesh(entity.Id) is not { } mesh) continue;
            draws.Add(new RenderDraw(mesh, entity.Transform.ToMatrix() * worldToClip,
                entity.IsVisible, entity.Id == selectedId));
        }
        return [.. draws];
    }

    public RenderDraw[] SnapshotGeographic(GeoRenderFrame frame, Guid? selectedId = null)
    {
        List<RenderDraw> draws = [];
        foreach (WorldEntity entity in world.Entities)
        {
            if (entity.Transform.Spatial.Space != SpatialSpace.Ecef ||
                GetActiveMesh(entity.Id) is not { } mesh) continue;
            draws.Add(new RenderDraw(mesh, frame.ModelToClip(entity.Transform),
                entity.IsVisible, entity.Id == selectedId));
        }
        return [.. draws];
    }

    public RenderMesh? GetActiveMesh(Guid id) =>
        _bindings.TryGetValue(id, out var binding) ? binding.ActiveMesh : null;

    public WorldRenderBridge CopyFor(WorldScene target)
    {
        var copy = new WorldRenderBridge(target);
        foreach (var (id, binding) in _bindings)
            if (target.Find(id) is not null) copy._bindings.Add(id, binding);
        return copy;
    }

    private void RequireEntity(Guid id)
    {
        if (world.Find(id) is null)
            throw new KeyNotFoundException($"World 实体不存在：{id}");
    }
}