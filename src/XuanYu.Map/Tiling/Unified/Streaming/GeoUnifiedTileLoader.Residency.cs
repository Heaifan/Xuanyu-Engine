namespace XuanYu.World.Maps;

public sealed partial class GeoUnifiedTileLoader
{
    private readonly HashSet<(int, int)> _evictable = [];

    internal void MarkEvictable(IEnumerable<(int South, int West)> keys)
    {
        lock (_gate) foreach (var key in keys) _evictable.Add(key);
    }

    internal bool TrimUnneeded(IReadOnlyCollection<(int South, int West)> needed)
    {
        lock (_gate)
        {
            var victims = _evictable.Where(key => !needed.Contains(key)).ToArray();
            if (victims.Length != 0 && _pins != 0) return false;
            foreach (var key in victims)
            {
                if (!_requests.TryGetValue(key, out var task) || !task.Task.IsCompletedSuccessfully) continue;
                _requests.Remove(key);
                _evictable.Remove(key);
                _reserved -= Reservation;
            }
            StartWaiting();
            return true;
        }
    }

    internal bool CanFitPage(int tileCount, long pageBytes) =>
        pageBytes <= _budget && tileCount <= (_budget - pageBytes) / Reservation;

    public bool Release(int south, int west)
    {
        lock (_gate)
        {
            var key = (south, west);
            if (_pins != 0 || !_requests.TryGetValue(key, out var task) || !task.Task.IsCompletedSuccessfully)
                return false;
            _requests.Remove(key);
            _evictable.Remove(key);
            _reserved -= Reservation;
            StartWaiting();
            return true;
        }
    }

    public bool TryReservePage(long bytes)
    {
        if (bytes <= 0) throw new ArgumentOutOfRangeException(nameof(bytes));
        lock (_gate)
        {
            if (bytes > _budget - _reserved) return false;
            _reserved += bytes;
            _pageBytes += bytes;
            return true;
        }
    }

    public void ReleasePage(long bytes)
    {
        lock (_gate)
        {
            if (bytes <= 0 || bytes > _pageBytes) throw new ArgumentOutOfRangeException(nameof(bytes));
            _pageBytes -= bytes;
            _reserved -= bytes;
            StartWaiting();
        }
    }
}
