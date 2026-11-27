namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedTileLoader
{
    private readonly NasademTileCatalog _catalog;
    internal NasademTileCatalog Catalog => _catalog;
    private readonly long _budget;
    private readonly int _maxQueued;
    private readonly object _gate = new();
    private readonly Dictionary<(int, int), Entry> _requests = [];
    private readonly Queue<(int South, int West)> _queue = new();
    private long _reserved, _pageBytes;
    private const long Reservation = NasademTile.ByteLength * 3;

    public GeoUnifiedTileLoader(NasademTileCatalog catalog, long budget, int maxQueued = 64)
    {
        if (maxQueued < 1) throw new ArgumentOutOfRangeException(nameof(maxQueued));
        _catalog = catalog;
        _budget = budget;
        _maxQueued = maxQueued;
    }

    public long ReservedBytes { get { lock (_gate) return _reserved; } }

    public Task<bool> Request(int south, int west)
    {
        lock (_gate)
        {
            var key = (south, west);
            if (_requests.TryGetValue(key, out var existing)) return existing.Task;
            if (!_catalog.Coordinates.Contains(key)) return Task.FromResult(false);
            if (Reservation > _budget)
                return Task.FromException<bool>(new InvalidOperationException("高程数据预算不足。"));
            if (_queue.Count >= _maxQueued)
                return Task.FromException<bool>(new InvalidOperationException("高程读取队列已满。"));
            var source = new Entry();
            _requests.Add(key, source);
            _queue.Enqueue(key);
            StartWaiting();
            return source.Task;
        }
    }

    private void StartWaiting()
    {
        while (_queue.Count > 0 && Reservation <= _budget - _reserved)
        {
            var key = _queue.Dequeue();
            _reserved += Reservation;
            _ = Task.Run(() => Load(key));
        }
    }

    private void Load((int South, int West) key)
    {
        try
        {
            var tile = _catalog.Load(key.South, key.West);
            var hash = tile is null ? null : Fingerprint(tile);
            lock (_gate)
            {
                var source = _requests[key];
                if (tile is null)
                {
                    _requests.Remove(key);
                    _reserved -= Reservation;
                    StartWaiting();
                    source.Completion.SetResult(false);
                    return;
                }
                CheckVersion(key, hash!);
                RecordSeams(tile);
                source.Tile = tile;
                source.Completion.SetResult(true);
            }
        }
        catch (Exception error)
        {
            lock (_gate)
            {
                var source = _requests[key];
                _requests.Remove(key);
                _reserved -= Reservation;
                StartWaiting();
                source.Completion.SetException(error);
            }
        }
    }

    private sealed class Entry
    {
        internal readonly TaskCompletionSource<bool> Completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal Task<bool> Task => Completion.Task;
        internal NasademTile? Tile;
    }
}