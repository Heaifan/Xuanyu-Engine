using System.Numerics;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed class G07ContourLabelCache
{
    private const long RefreshIntervalMs = 180;
    private (GeoUnifiedLodSession Cpu, GeoUnifiedCoverage Cover, Matrix4x4 Mvp,
        uint Width, uint Height, int Interval, float Scale)? _key;
    private GeoContourLabel[] _labels = [];
    private long _lastRefresh;
    private bool _needsRefill;

    public GeoContourLabel[] Update(GeoTerrainRenderSession source, GeoUnifiedLodSession cpu,
        Matrix4x4 mvp, uint width, uint height, int interval, float scale, long now)
    {
        if (interval <= 0 || width < 200 || height < 150)
        {
            Clear();
            return _labels;
        }
        var key = (cpu, cpu.Coverage, mvp, width, height, interval, scale);
        bool newDataset = _key is not { } previous || !ReferenceEquals(previous.Cpu, cpu);
        if (newDataset) Clear();
        bool changed = _key is not { } old || old != key;
        bool intervalChanged = _key is { } current && current.Interval != interval;
        _needsRefill |= changed;
        bool refill = newDataset || intervalChanged ||
            (_needsRefill && now - _lastRefresh >= RefreshIntervalMs);
        if (!changed && !refill) return _labels;

        bool hadLabels = _labels.Length > 0;
        var next = source.BuildG07Labels(cpu, mvp, width, height, interval, scale, _labels, refill);
        if (!refill && hadLabels && next.Length == 0)
        {
            // 旧地表上的标高全部失效时，不能继续等待刷新间隔。
            next = source.BuildG07Labels(cpu, mvp, width, height, interval, scale);
            refill = true;
        }
        if (refill)
        {
            _lastRefresh = now;
            _needsRefill = false;
        }
        _key = key;
        _labels = next;
        return _labels;
    }

    public void Clear()
    {
        _key = null;
        _labels = [];
        _lastRefresh = 0;
        _needsRefill = false;
    }
}