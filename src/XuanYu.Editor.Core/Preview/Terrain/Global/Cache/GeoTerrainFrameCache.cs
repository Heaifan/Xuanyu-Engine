using System.Diagnostics.CodeAnalysis;

namespace XuanYu.Editor.Preview.Terrain;

internal sealed class GeoTerrainFrameCache<TKey, TValue> where TKey : notnull
{
    private sealed class Entry(TValue value, long frame)
    {
        public TValue Value { get; } = value;
        public long LastFrame { get; set; } = frame;
    }

    private readonly Dictionary<TKey, Entry> _entries = [];
    private long _frame;
    public int Count => _entries.Count;
    public IReadOnlyCollection<TKey> CachedKeys => _entries.Keys;
    public int ActiveCount { get; private set; }
    public long EvictedCount { get; private set; }

    public void BeginFrame()
    {
        _frame++;
        ActiveCount = 0;
    }

    public TValue this[TKey key] => Touch(_entries[key]);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (_entries.TryGetValue(key, out Entry? item))
        {
            value = Touch(item);
            return true;
        }
        value = default;
        return false;
    }

    public void Add(TKey key, TValue value)
    {
        _entries.Add(key, new Entry(value, _frame));
        ActiveCount++;
    }

    private TValue Touch(Entry item)
    {
        if (item.LastFrame != _frame)
        {
            item.LastFrame = _frame;
            ActiveCount++;
        }
        return item.Value;
    }

    public void TrimInactive(int retained)
    {
        int excess = Count - ActiveCount - retained;
        if (excess <= 0) return;
        // 当前帧访问过的项包含父片和未完成替换的子片，不可回收。
        TKey[] expired = _entries.Where(pair => pair.Value.LastFrame != _frame)
            .OrderBy(pair => pair.Value.LastFrame).Take(excess)
            .Select(pair => pair.Key).ToArray();
        foreach (TKey key in expired) _entries.Remove(key);
        EvictedCount += expired.Length;
    }
}