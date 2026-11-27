namespace XuanYu.Editor.Preview.Terrain;

internal sealed class GeoTerrainNativeMeshCache<TKey, TValue>(int capacity) where TKey : notnull
{
    private sealed class Entry(TValue value, long lastUse)
    {
        public TValue Value { get; } = value;
        public long LastUse { get; set; } = lastUse;
    }

    private readonly Dictionary<TKey, Entry> _entries = [];
    private HashSet<TKey> _active = [];
    private long _clock;
    public int Count => _entries.Count;
    public long EvictedCount { get; private set; }
    public long ActiveEvictionCount { get; private set; }
    public long RejectedInsertCount { get; private set; }

    public void BeginFrame(IEnumerable<TKey> active) => _active = active.ToHashSet();

    public bool TryGet(TKey key, out TValue value)
    {
        if (!_entries.TryGetValue(key, out Entry? entry))
        {
            value = default!;
            return false;
        }
        entry.LastUse = ++_clock;
        value = entry.Value;
        return true;
    }

    public TValue Get(TKey key) => TryGet(key, out TValue value)
        ? value : throw new KeyNotFoundException();

    public bool ContainsMatching(Func<TKey, bool> predicate) =>
        _entries.Keys.Any(predicate);

    public bool Set(TKey key, TValue value)
    {
        if (_entries.TryGetValue(key, out Entry? current))
        {
            current.LastUse = ++_clock;
            _entries[key] = new Entry(value, _clock);
            return true;
        }
        if (_entries.Count == capacity)
        {
            var candidates = _entries.Where(pair => !_active.Contains(pair.Key)).ToArray();
            if (candidates.Length == 0)
            {
                RejectedInsertCount++;
                return false;
            }
            TKey victim = candidates.MinBy(pair => pair.Value.LastUse).Key;
            if (_active.Contains(victim)) ActiveEvictionCount++;
            _entries.Remove(victim);
            EvictedCount++;
        }
        _entries.Add(key, new Entry(value, ++_clock));
        return true;
    }
}
