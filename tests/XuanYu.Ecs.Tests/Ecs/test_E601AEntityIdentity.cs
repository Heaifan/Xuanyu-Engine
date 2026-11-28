using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601AEntityIdentity
{
    [Fact]
    public void Create_RegistersBothDirections()
    {
        var registry = new EntityRegistry();
        EntityId id = registry.Create();
        Assert.True(registry.TryGetGuid(id, out Guid guid));
        Assert.NotEqual(Guid.Empty, guid);
        Assert.True(registry.TryGetId(guid, out EntityId found));
        Assert.Equal(id, found);
        Assert.True(registry.IsAlive(id));
        Assert.Equal(1, registry.Count);
    }

    [Fact]
    public void Create_WithExistingGuid_RejectsWithoutChangingState()
    {
        var registry = new EntityRegistry();
        Guid guid = Guid.NewGuid();
        EntityId id = registry.Create(guid);
        Assert.Throws<ArgumentException>(() => registry.Create(guid));
        Assert.Throws<ArgumentException>(() => registry.Create(Guid.Empty));
        Assert.Equal(1, registry.Count);
        Assert.True(registry.TryGetId(guid, out EntityId found));
        Assert.Equal(id, found);
        Assert.True(registry.TryGetGuid(id, out Guid actual));
        Assert.Equal(guid, actual);
    }

    [Fact]
    public void InvalidHandles_AreNotAlive()
    {
        var registry = new EntityRegistry();
        EntityId live = registry.Create();
        Assert.False(registry.IsAlive(default));
        Assert.False(registry.IsAlive(new EntityId(-1, 1)));
        Assert.False(registry.IsAlive(new EntityId(100, 1)));
        Assert.False(registry.IsAlive(new EntityId(live.Index, 0)));
        Assert.False(registry.TryGetGuid(default, out Guid guid));
        Assert.Equal(Guid.Empty, guid);
        Assert.False(registry.Destroy(default));
        Assert.Equal(1, registry.Count);
    }

    [Fact]
    public void SeparateRegistries_CanRestoreSamePersistentIdentity()
    {
        var editor = new EntityRegistry();
        var runtime = new EntityRegistry();
        Guid guid = Guid.NewGuid();
        EntityId editorId = editor.Create(guid);
        EntityId runtimeId = runtime.Create(guid);
        Assert.True(runtime.Destroy(runtimeId));
        Assert.True(editor.IsAlive(editorId));
        Assert.True(editor.TryGetId(guid, out EntityId found));
        Assert.Equal(editorId, found);
        Assert.False(runtime.TryGetId(guid, out _));
        Assert.Equal(1, editor.Count);
        Assert.Equal(0, runtime.Count);
    }
}