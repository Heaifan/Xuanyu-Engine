using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601AEntityLifecycle
{
    [Fact]
    public void Destroy_RemovesBothDirectionsAndInvalidatesHandle()
    {
        var registry = new EntityRegistry();
        Guid guid = Guid.NewGuid();
        EntityId id = registry.Create(guid);
        Assert.True(registry.Destroy(id));
        Assert.False(registry.IsAlive(id));
        Assert.False(registry.TryGetId(guid, out _));
        Assert.False(registry.TryGetGuid(id, out _));
        Assert.False(registry.Destroy(id));
        Assert.Equal(0, registry.Count);
    }

    [Fact]
    public void Reuse_DoesNotReviveOldHandle()
    {
        var registry = new EntityRegistry();
        EntityId old = registry.Create();
        Assert.True(registry.Destroy(old));
        Guid guid = Guid.NewGuid();
        EntityId current = registry.Create(guid);
        Assert.Equal(old.Index, current.Index);
        Assert.NotEqual(old.Generation, current.Generation);
        Assert.False(registry.Destroy(old));
        Assert.False(registry.TryGetGuid(old, out _));
        Assert.True(registry.TryGetGuid(current, out Guid found));
        Assert.Equal(guid, found);
        Assert.Equal(1, registry.Count);
    }

    [Fact]
    public void Restore_SameGuidGetsNewRuntimeHandle()
    {
        var registry = new EntityRegistry();
        Guid guid = Guid.NewGuid();
        EntityId old = registry.Create(guid);
        Assert.True(registry.Destroy(old));
        EntityId restored = registry.Create(guid);
        Assert.NotEqual(old, restored);
        Assert.False(registry.IsAlive(old));
        Assert.True(registry.TryGetId(guid, out EntityId found));
        Assert.Equal(restored, found);
    }

    [Fact]
    public void Churn_PreservesSurvivorsAndRejectsEveryRetiredHandle()
    {
        const int count = 4096;
        var registry = new EntityRegistry();
        var original = new (Guid Guid, EntityId Id)[count];
        for (int i = 0; i < count; i++)
        {
            Guid guid = Guid.NewGuid();
            original[i] = (guid, registry.Create(guid));
        }
        for (int i = 0; i < count; i += 2)
            Assert.True(registry.Destroy(original[i].Id));
        Assert.Equal(count / 2, registry.Count);
        for (int i = 0; i < count / 2; i++)
        {
            EntityId replacement = registry.Create();
            Assert.InRange(replacement.Index, 0, count - 1);
            Assert.True(registry.TryGetGuid(replacement, out Guid guid));
            Assert.True(registry.TryGetId(guid, out EntityId found));
            Assert.Equal(replacement, found);
        }
        for (int i = 0; i < count; i++)
        {
            var (guid, id) = original[i];
            if (i % 2 == 0)
            {
                Assert.False(registry.IsAlive(id));
                Assert.False(registry.TryGetId(guid, out _));
                continue;
            }
            Assert.True(registry.TryGetGuid(id, out Guid found));
            Assert.Equal(guid, found);
            Assert.True(registry.TryGetId(guid, out EntityId actual));
            Assert.Equal(id, actual);
        }
        Assert.Equal(count, registry.Count);
    }
}