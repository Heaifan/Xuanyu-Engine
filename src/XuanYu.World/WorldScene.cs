namespace XuanYu.World;

public sealed class WorldScene
{
    private readonly Dictionary<Guid, WorldEntity> _entities = [];

    public IReadOnlyCollection<WorldEntity> Entities =>
        _entities.Values.ToArray();

    public WorldEntity Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var entity = new WorldEntity(Guid.NewGuid(), name.Trim(),
            true, Transform.Identity);
        _entities.Add(entity.Id, entity);
        return entity;
    }

    public WorldEntity? Find(Guid id) =>
        _entities.GetValueOrDefault(id);

    public WorldEntity Rename(Guid id, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        WorldEntity original = _entities[id];
        string trimmed = name.Trim();
        if (_entities.Values.Any(x => x.Id != id && x.Name == trimmed))
            throw new InvalidOperationException("场景中已经存在同名实体。");
        WorldEntity updated = original with { Name = trimmed };
        _entities[id] = updated;
        return updated;
    }

    public WorldEntity SetTransform(Guid id, Transform transform)
    {
        WorldEntity updated = _entities[id] with { Transform = transform };
        _entities[id] = updated;
        return updated;
    }

    public WorldEntity SetVisible(Guid id, bool visible)
    {
        WorldEntity updated = _entities[id] with { IsVisible = visible };
        _entities[id] = updated;
        return updated;
    }

    public bool Delete(Guid id) => _entities.Remove(id);

    public void Replace(IReadOnlyList<WorldEntity> entities)
    {
        _entities.Clear();
        foreach (WorldEntity entity in entities)
            _entities.Add(entity.Id, entity);
    }
}