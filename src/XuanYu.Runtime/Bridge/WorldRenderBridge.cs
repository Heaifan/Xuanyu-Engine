using XuanYu.Render;
using XuanYu.World;

namespace XuanYu.Runtime;

public sealed partial class WorldRenderBridge(WorldScene world)
{
    private readonly WorldScene _world = world;
    private readonly Dictionary<Guid, EntityRenderBinding> _bindings = [];

    public void Bind(Guid id, RenderMesh mesh) =>
        BindRepresentation(id, EntityRepresentationKind.Mesh, mesh);

    public void BindRepresentation(Guid id, EntityRepresentationKind kind, RenderMesh mesh)
    {
        RequireEntity(id);
        ArgumentNullException.ThrowIfNull(mesh);
        if (kind is not (EntityRepresentationKind.Mesh or EntityRepresentationKind.Proxy or
            EntityRepresentationKind.Point or EntityRepresentationKind.Symbol))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (!_bindings.TryGetValue(id, out var binding))
            binding = new(null, null, kind);
        _bindings[id] = kind switch
        {
            EntityRepresentationKind.Mesh => binding with { Mesh = mesh },
            EntityRepresentationKind.Proxy => binding with { Proxy = mesh },
            EntityRepresentationKind.Point => binding with { Point = mesh },
            _ => binding with { Symbol = mesh }
        };
    }

    public EntityRepresentationKind GetRepresentation(Guid id)
    {
        RequireEntity(id);
        return _bindings.TryGetValue(id, out var binding) ? binding.Active : EntityRepresentationKind.None;
    }

    public void SetRepresentation(Guid id, EntityRepresentationKind kind)
    {
        RequireEntity(id);
        if (!Enum.IsDefined(kind))
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
        if (_world.Find(id) is null)
            throw new KeyNotFoundException($"World 实体不存在：{id}");
    }
}