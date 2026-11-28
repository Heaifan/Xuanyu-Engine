using Xunit;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601CQueries
{
    [Fact]
    public void Query_RequiresAllTypesAndIncludesSupersets()
    {
        var world = new EcsWorld();
        var empty = world.Create();
        var a = world.Create();
        var b = world.Create();
        var c = world.Create();
        world.Add(a, 1);
        world.Add(b, 2);
        world.Add(b, 2d);
        world.Add(c, 3);
        world.Add(c, 3d);
        world.Add(c, 3L);
        Assert.Equal(4, world.Query().Sum(table => table.Count));
        Assert.Equal(3, world.Query(ComponentType.Of<int>()).Sum(table => table.Count));
        Assert.Equal(2, world.Query(ComponentType.Of<int>(), ComponentType.Of<double>()).Sum(table => table.Count));
        var only = Assert.Single(world.Query(ComponentType.Of<int>(), ComponentType.Of<double>(), ComponentType.Of<long>()));
        Assert.Equal(c, Assert.Single(only.Entities.ToArray()));
        Assert.Empty(world.Query(ComponentType.Of<float>()));
        Assert.True(world.IsAlive(empty));
    }

    [Fact]
    public void Query_ColumnWritesUpdateRealComponents()
    {
        var world = new EcsWorld();
        var a = world.Create();
        var b = world.Create();
        world.Add(a, 10);
        world.Add(a, 2d);
        world.Add(b, 20);
        world.Add(b, 3d);
        foreach (var table in world.Query(ComponentType.Of<int>(), ComponentType.Of<double>()))
        {
            var values = table.GetColumn<int>();
            var factors = table.GetColumn<double>();
            for (int row = 0; row < table.Count; row++) values[row] *= (int)factors[row];
        }
        Assert.Equal(20, world.Get<int>(a));
        Assert.Equal(60, world.Get<int>(b));
    }

    [Fact]
    public void Query_SkipsEmptyTablesAndAcceptsRepeatedRequirements()
    {
        var world = new EcsWorld();
        var id = world.Create();
        world.Add(id, 1);
        world.Add(id, 2d);
        Assert.Single(world.Query(ComponentType.Of<int>(), ComponentType.Of<int>()));
        world.Remove<double>(id);
        Assert.Empty(world.Query(ComponentType.Of<double>()));
        world.Destroy(id);
        Assert.Empty(world.Query());
    }

    [Fact]
    public void Batch_MigrationAndDestructionKeepRowsAndValuesAligned()
    {
        const int count = 4096;
        var world = new EcsWorld();
        var ids = new EntityId[count];
        for (int i = 0; i < count; i++)
        {
            ids[i] = world.Create();
            world.Add(ids[i], i);
            if (i % 2 == 0) world.Add(ids[i], i + 0.5d);
        }
        for (int i = 0; i < count; i++)
            if (i % 4 == 0) Assert.True(world.Remove<double>(ids[i]));
            else if (i % 4 == 1) Assert.True(world.Add(ids[i], i + 0.5d));
        for (int i = 0; i < count; i += 4) Assert.True(world.Destroy(ids[i]));
        Assert.Equal(3072, world.Count);
        Assert.Equal(2048, world.Query(ComponentType.Of<double>()).Sum(table => table.Count));
        var seen = new HashSet<EntityId>();
        foreach (var table in world.Query(ComponentType.Of<int>()))
        {
            var values = table.GetColumn<int>();
            for (int row = 0; row < table.Count; row++)
            {
                int i = values[row];
                Assert.NotEqual(0, i % 4);
                Assert.Equal(ids[i], table.Entities[row]);
                Assert.True(seen.Add(table.Entities[row]));
                Assert.Equal(i, world.Get<int>(ids[i]));
                if (i % 4 != 3) Assert.Equal(i + 0.5d, world.Get<double>(ids[i]));
                else Assert.False(world.Has<double>(ids[i]));
            }
        }
        Assert.Equal(world.Count, seen.Count);
    }
}