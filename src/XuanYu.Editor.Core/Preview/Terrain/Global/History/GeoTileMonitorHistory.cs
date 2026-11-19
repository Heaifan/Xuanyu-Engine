namespace XuanYu.Editor.Preview.Terrain;

public sealed class GeoTileMonitorHistory
{
    public const int Capacity = 600;
    private readonly GeoTileMonitorFrame?[] _frames = new GeoTileMonitorFrame?[Capacity];
    private int _start;
    private int _count;
    public bool IsRecording { get; private set; }
    public int Count => _count;

    public GeoTileMonitorFrame this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_count)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _frames[(_start + index) % Capacity]!;
        }
    }

    public void SetRecording(bool enabled) => IsRecording = enabled;

    public void Clear()
    {
        Array.Clear(_frames);
        _start = 0;
        _count = 0;
    }

    internal void Append(GeoTileMonitorFrame frame)
    {
        if (!IsRecording) return;
        _frames[(_start + _count) % Capacity] = frame;
        if (_count < Capacity) _count++;
        else _start = (_start + 1) % Capacity;
    }
}