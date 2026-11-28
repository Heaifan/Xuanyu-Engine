using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601BArchetypeRows
{
    private readonly record struct Position(float X, float Y);
    private readonly record struct Velocity(float X, float Y);

    [Fact]
    public void Append_InitializesIndependentTypedColumns()
    {
        var registry = new EntityRegistry();
        var a = new Archetype(ComponentType.Of<Position>(), ComponentType.Of<Velocity>());
        EntityId first = registry.Create();
        Assert.Equal(0, a.Append(first));
        a.GetColumn<Position>()[0] = new(2, 3);
        a.GetColumn<Velocity>()[0] = new(4, 5);
        EntityId second = registry.Create();
        Assert.Equal(1, a.Append(second));
        Assert.Equal(new Position(2, 3), a.GetColumn<Position>()[0]);
        Assert.Equal(new Velocity(4, 5), a.GetColumn<Velocity>()[0]);
        Assert.Equal(default(Position), a.GetColumn<Position>()[1]);
        Assert.Equal(default(Velocity), a.GetColumn<Velocity>()[1]);
        Assert.Equal(new[] { first, second }, a.Entities.ToArray());
    }

    [Fact]
    public void RemoveMiddle_MovesEveryColumnTogetherWithoutDestroyingEntity()
    {
        var registry = new EntityRegistry();
        var a = new Archetype(ComponentType.Of<int>(), ComponentType.Of<double>());
        EntityId first = registry.Create(), middle = registry.Create(), last = registry.Create();
        a.Append(first); a.Append(middle); a.Append(last);
        a.GetColumn<int>()[0] = 10; a.GetColumn<int>()[1] = 20; a.GetColumn<int>()[2] = 30;
        a.GetColumn<double>()[0] = 1.5; a.GetColumn<double>()[1] = 2.5; a.GetColumn<double>()[2] = 3.5;
        Assert.Equal<EntityId?>(last, a.RemoveAtSwapBack(1));
        Assert.Equal(new[] { first, last }, a.Entities.ToArray());
        Assert.Equal(new[] { 10, 30 }, a.GetColumn<int>().ToArray());
        Assert.Equal(new[] { 1.5, 3.5 }, a.GetColumn<double>().ToArray());
        Assert.True(registry.IsAlive(middle));
        Assert.Equal(3, registry.Count);
    }

    [Fact]
    public void RemoveLast_ThenAppend_DoesNotRetainOldValue()
    {
        var registry = new EntityRegistry();
        var a = new Archetype(ComponentType.Of<long>());
        EntityId first = registry.Create(), last = registry.Create();
        a.Append(first); a.Append(last);
        a.GetColumn<long>()[0] = 7; a.GetColumn<long>()[1] = 8;
        Assert.Null(a.RemoveAtSwapBack(1));
        Assert.Equal(1, a.Append(registry.Create()));
        Assert.Equal(new long[] { 7, 0 }, a.GetColumn<long>().ToArray());
    }

    [Fact]
    public void InvalidRow_DoesNotChangeStorage()
    {
        var registry = new EntityRegistry();
        var a = new Archetype(ComponentType.Of<int>());
        a.Append(registry.Create());
        a.GetColumn<int>()[0] = 9;
        Assert.Throws<ArgumentOutOfRangeException>(() => a.RemoveAtSwapBack(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => a.RemoveAtSwapBack(1));
        Assert.Equal(1, a.Count);
        Assert.Equal(9, a.GetColumn<int>()[0]);
    }

    [Fact]
    public void SameSchema_SeparateArchetypesHaveIndependentStorage()
    {
        var registry = new EntityRegistry();
        ComponentType type = ComponentType.Of<int>();
        var a = new Archetype(type);
        var b = new Archetype(type);
        a.Append(registry.Create()); b.Append(registry.Create());
        a.GetColumn<int>()[0] = 42;
        Assert.Equal(0, b.GetColumn<int>()[0]);
        Assert.Null(a.RemoveAtSwapBack(0));
        Assert.Equal(1, b.Count);
    }
}