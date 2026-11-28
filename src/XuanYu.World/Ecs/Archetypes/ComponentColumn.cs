using System.Runtime.InteropServices;

namespace XuanYu.World.Ecs;

internal sealed class ComponentColumn<T> : IComponentColumn where T : unmanaged
{
    private readonly List<T> _values = [];

    public Span<T> Values => CollectionsMarshal.AsSpan(_values);

    public void AppendDefault() => _values.Add(default);

    public void RemoveAtSwapBack(int row)
    {
        int last = _values.Count - 1;
        if (row != last) _values[row] = _values[last];
        _values.RemoveAt(last);
    }
}