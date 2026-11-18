namespace XuanYu.World.Maps;

internal sealed class NasademMiddleCache(int capacity)
{
    private readonly Dictionary<(int South, int West), LinkedListNode<NasademOverviewGrid>> _items = [];
    private readonly LinkedList<NasademOverviewGrid> _recent = new();
    public int Capacity => capacity;
    public int Count => _items.Count;
    public long Bytes => (long)Count * NasademOverviewLayout.Side(NasademOverviewLevel.Middle)
        * NasademOverviewLayout.Side(NasademOverviewLevel.Middle) * sizeof(short);

    public NasademOverviewGrid? Get(int south, int west)
    {
        if (!_items.TryGetValue((south, west), out var node)) return null;
        if (!ReferenceEquals(node, _recent.First))
        {
            _recent.Remove(node);
            _recent.AddFirst(node);
        }
        return node.Value;
    }

    public void Add(NasademOverviewGrid grid)
    {
        if (_items.Count == capacity)
        {
            var old = _recent.Last!;
            _items.Remove((old.Value.Info.SouthLatitude, old.Value.Info.WestLongitude));
            _recent.RemoveLast();
        }
        _items.Add((grid.Info.SouthLatitude, grid.Info.WestLongitude), _recent.AddFirst(grid));
    }
}