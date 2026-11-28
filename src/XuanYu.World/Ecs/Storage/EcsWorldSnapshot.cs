namespace XuanYu.World.Ecs;

public sealed partial class EcsWorld
{
    // 调用期间源世界结构及组件值须保持稳定。
    public EcsWorld Clone()
    {
        var snapshot = new EcsWorld();
        foreach (Archetype source in _catalog.Tables)
        {
            if (source.Count == 0) continue;
            Archetype target = snapshot._catalog.AppendSnapshotSchema(source);
            for (int row = 0; row < source.Count; row++)
            {
                _registry.TryGetGuid(source.Entities[row], out Guid guid);
                EntityId id = snapshot.Create(guid, target);
                source.CopySharedTo(row, target, snapshot._locations[id.Index].Row);
            }
        }
        return snapshot;
    }
}