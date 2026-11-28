using XuanYu.World.Ecs;
using Xunit;

namespace XuanYu.World.Tests.Ecs;

public sealed class test_E601BArchetypeBatch
{
    private readonly record struct Sample(int Index, int Value);

    [Fact]
    public void DenseColumns_StayAlignedAcrossGrowthAndSwapRemoval()
    {
        const int size = 4096;
        var registry = new EntityRegistry();
        var a = new Archetype(ComponentType.Of<Sample>(), ComponentType.Of<long>());
        for (int i = 0; i < size; i++)
        {
            EntityId id = registry.Create();
            int row = a.Append(id);
            a.GetColumn<Sample>()[row] = new(id.Index, i + 1);
            a.GetColumn<long>()[row] = i * 10L;
        }
        Span<long> values = a.GetColumn<long>();
        for (int i = 0; i < values.Length; i++) values[i] += 5;
        for (int i = 0; i < size / 2; i++)
        {
            int row = i % a.Count;
            EntityId removed = a.Entities[row];
            EntityId last = a.Entities[^1];
            bool isLast = row == a.Count - 1;
            EntityId? moved = a.RemoveAtSwapBack(row);
            if (isLast) Assert.Null(moved);
            else Assert.Equal<EntityId?>(last, moved);
            Assert.True(registry.Destroy(removed));
        }
        Assert.Equal(size / 2, a.Count);
        Assert.Equal(a.Count, registry.Count);
        Assert.Equal(a.Count, a.GetColumn<Sample>().Length);
        Assert.Equal(a.Count, a.GetColumn<long>().Length);
        for (int row = 0; row < a.Count; row++)
        {
            EntityId id = a.Entities[row];
            Sample sample = a.GetColumn<Sample>()[row];
            Assert.True(registry.IsAlive(id));
            Assert.Equal(id.Index, sample.Index);
            Assert.Equal(id.Index + 1, sample.Value);
            Assert.Equal(id.Index * 10L + 5, a.GetColumn<long>()[row]);
        }
        while (a.Count > 0)
        {
            EntityId id = a.Entities[^1];
            Assert.Null(a.RemoveAtSwapBack(a.Count - 1));
            Assert.True(registry.Destroy(id));
        }
        Assert.Equal(0, registry.Count);
        Assert.True(a.Entities.IsEmpty);
        Assert.True(a.GetColumn<Sample>().IsEmpty);
        Assert.True(a.GetColumn<long>().IsEmpty);
    }
}