namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedTileLoader
{
    private readonly NasademTileCatalog _catalog;
    internal NasademTileCatalog Catalog => _catalog;
    private readonly long _budget;
    private readonly int _maxQueued;
    private readonly object _gate = new();
    private readonly Dictionary<(int, int), TaskCompletionSource<NasademTile?>> _requests = [];
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

    public Task<NasademTile?> Request(int south, int west)
    {
        lock (_gate)
        {
            var key = (south, west);
            if (_requests.TryGetValue(key, out var existing)) return existing.Task;
            if (!_catalog.Coordinates.Contains(key)) return Task.FromResult<NasademTile?>(null);
            if (Reservation > _budget)
                return Task.FromException<NasademTile?>(new InvalidOperationException("高程数据预算不足。"));
            if (_queue.Count >= _maxQueued)
                return Task.FromException<NasademTile?>(new InvalidOperationException("高程读取队列已满。"));
            var source = new TaskCompletionSource<NasademTile?>(TaskCreationOptions.RunContinuationsAsynchronously);
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
                if (hash is not null) CheckVersion(key, hash);
                if (tile is not null) RecordSeams(tile);
                _requests[key].SetResult(tile);
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
                source.SetException(error);
            }
        }
    }
}
