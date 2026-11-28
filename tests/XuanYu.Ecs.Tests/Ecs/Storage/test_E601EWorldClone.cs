using Xunit;
using XuanYu.World.Ecs;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601EWorldClone
{
    private readonly record struct Sample(int Value, double Weight);

    [Fact]
    public void EmptyWorld_ClonesWithoutSharingLifecycle()
    {
        var source = new EcsWorld();
        var clone = source.Clone();
        Assert.Empty(clone.Query());
        clone.Create();
        Assert.Equal(0, source.Count);
        Assert.Equal(1, clone.Count);
    }

    [Fact]
    public void Clone_PreservesAllSchemasAndPermanentIdentities()
    {
        var source = new EcsWorld();
        Guid a = Guid.NewGuid(), b = Guid.NewGuid(), c = Guid.NewGuid();
        source.Create(a);
        var second = source.Create(b);
        var third = source.Create(c);
        source.Add(second, new Sample(3, 2.5));
        source.Add(third, new Sample(7, 9));
        source.Add(third, 44);
        var clone = source.Clone();
        Assert.Equal(3, clone.Count);
        Assert.True(clone.TryGetId(a, out var empty));
        Assert.False(clone.Has<Sample>(empty));
        Assert.True(clone.TryGetId(b, out var one));
        Assert.Equal(new Sample(3, 2.5), clone.Get<Sample>(one));
        Assert.True(clone.TryGetId(c, out var two));
        Assert.Equal(new Sample(7, 9), clone.Get<Sample>(two));
        Assert.Equal(44, clone.Get<int>(two));
        Assert.Equal(new[] { 1, 1, 1 }, clone.Query().Select(table => table.Count).ToArray());
    }

    [Fact]
    public void Clone_SeparatesColumnsAndStructuralChanges()
    {
        var source = new EcsWorld();
        Guid guid = Guid.NewGuid();
        var id = source.Create(guid);
        source.Add(id, 10);
        var clone = source.Clone();
        Assert.True(clone.TryGetId(guid, out var copy));
        var table = Assert.Single(clone.Query(ComponentType.Of<int>()));
        table.GetColumn<int>()[0] = 90;
        Assert.Equal(10, source.Get<int>(id));
        source.Set(id, 20);
        Assert.Equal(90, clone.Get<int>(copy));
        Assert.True(clone.Add(copy, 2.5));
        Assert.True(clone.Remove<int>(copy));
        Assert.True(source.Has<int>(id));
        Assert.False(source.Has<double>(id));
        Assert.True(clone.Destroy(copy));
        Assert.True(source.IsAlive(id));
    }

    [Fact]
    public void Clone_BatchPreservesValuesAfterSourceIndexReuse()
    {
        var source = new EcsWorld();
        var guids = new Guid[4096];
        for (int i = 0; i < guids.Length; i++)
        {
            guids[i] = Guid.NewGuid();
            var id = source.Create(guids[i]);
            source.Add(id, i);
            if (i % 2 == 0) source.Add(id, new Sample(i, i + 0.5));
        }
        for (int i = 0; i < guids.Length; i += 4)
        {
            Assert.True(source.TryGetId(guids[i], out var id));
            Assert.True(source.Destroy(id));
            source.Create();
        }
        var clone = source.Clone();
        Assert.Equal(4096, clone.Count);
        for (int i = 0; i < guids.Length; i++)
        {
            Assert.Equal(i % 4 != 0, clone.TryGetId(guids[i], out var id));
            if (i % 4 == 0) continue;
            Assert.Equal(i, clone.Get<int>(id));
            Assert.Equal(i % 2 == 0, clone.Has<Sample>(id));
            if (i % 2 == 0) Assert.Equal(new Sample(i, i + 0.5), clone.Get<Sample>(id));
        }
    }
}