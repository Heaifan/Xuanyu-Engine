using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601BArchetypeSchema
{
    private readonly record struct Position(double X, double Y);
    private readonly record struct Velocity(double X, double Y);

    [Fact]
    public void Schema_UsesExactComponentTypes()
    {
        var archetype = new Archetype(ComponentType.Of<Position>(),
            ComponentType.Of<Velocity>(), ComponentType.Of<int>());
        Assert.Equal(3, archetype.ComponentCount);
        Assert.True(archetype.Has<Position>());
        Assert.True(archetype.Has<Velocity>());
        Assert.True(archetype.Has<int>());
        Assert.False(archetype.Has<double>());
        Assert.True(archetype.GetColumn<Position>().IsEmpty);
        Assert.True(archetype.Entities.IsEmpty);
    }

    [Fact]
    public void DuplicateType_IsRejected()
    {
        Assert.Throws<ArgumentException>(() => new Archetype(
            ComponentType.Of<int>(), ComponentType.Of<int>()));
    }

    [Fact]
    public void NullSchema_IsRejected()
    {
        Assert.Throws<ArgumentNullException>(() => new Archetype(null!));
        Assert.Throws<ArgumentNullException>(() => new Archetype(new ComponentType[] { null! }));
    }

    [Fact]
    public void MissingColumn_IsRejected()
    {
        var archetype = new Archetype(ComponentType.Of<Position>());
        Assert.Throws<KeyNotFoundException>(() => { _ = archetype.GetColumn<Velocity>().Length; });
    }

    [Fact]
    public void EmptySchema_StoresEntitiesWithoutComponents()
    {
        var registry = new EntityRegistry();
        var archetype = new Archetype();
        EntityId first = registry.Create(), second = registry.Create();
        Assert.Equal(0, archetype.Append(first));
        Assert.Equal(1, archetype.Append(second));
        Assert.Equal(0, archetype.ComponentCount);
        Assert.Equal<EntityId?>(second, archetype.RemoveAtSwapBack(0));
        Assert.Equal(second, archetype.Entities[0]);
        Assert.Null(archetype.RemoveAtSwapBack(0));
        Assert.Equal(0, archetype.Count);
    }
}