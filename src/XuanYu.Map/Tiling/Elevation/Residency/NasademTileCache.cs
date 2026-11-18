namespace XuanYu.World.Maps;

internal sealed partial class NasademTileCache
{
    private readonly NasademTileCatalog _catalog;
    private readonly int _capacity;
    private readonly Dictionary<(int South, int West), LinkedListNode<NasademTile>> _tiles = [];
    private readonly LinkedList<NasademTile> _recent = new();
    private long _loaded, _evicted;

    public NasademTileCache(NasademTileCatalog catalog, int capacity)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        _catalog = catalog;
        _capacity = capacity;
    }

    public NasademSourceResidency Stats => new(true, _capacity, _tiles.Count, _loaded, _evicted);

    public NasademTile? Get(int south, int west)
    {
        var key = (south, west);
        if (Find(key) is { } cached) return cached;
        if (_pending is not null && _pendingKey == key) return CompletePending();
        var tile = _catalog.Load(south, west);
        if (tile is not null) Store(tile);
        return tile;
    }

    private NasademTile? Find((int South, int West) key)
    {
        if (!_tiles.TryGetValue(key, out var node)) return null;
        if (!ReferenceEquals(node, _recent.First))
        {
            _recent.Remove(node);
            _recent.AddFirst(node);
        }
        return node.Value;
    }

    private void Store(NasademTile tile)
    {
        if (_tiles.Count == _capacity)
        {
            var old = _recent.Last!;
            _tiles.Remove((old.Value.SouthLatitude, old.Value.WestLongitude));
            _recent.RemoveLast();
            _evicted++;
        }
        _tiles.Add((tile.SouthLatitude, tile.WestLongitude), _recent.AddFirst(tile));
        _loaded++;
    }
}