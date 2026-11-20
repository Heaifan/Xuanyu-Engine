namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoContourSurface
{
    public long CenterCount { get; private set; }
    public long CenterMissCount { get; private set; }
    public long SideCount { get; private set; }
    public long SideMissCount { get; private set; }
    public long SkippedBandCount { get; private set; }
    public long BoundsMissCount { get; private set; }
    public long BucketMissCount { get; private set; }
    public long TriangleMissCount { get; private set; }
    public long RecoveryCount { get; private set; }
    private string _firstMiss = "none";

    private void CountLookup(bool center, float x, float y, float? height)
    {
        if (center)
        {
            CenterCount++;
            if (height is not null) return;
            CenterMissCount++;
            if (CenterMissCount == 1) _firstMiss = $"({x:F2},{y:F2})";
        }
        else
        {
            SideCount++;
            if (height is null) SideMissCount++;
        }
    }

    public void CountSkippedBand() => SkippedBandCount++;

    public string DiagnosticSummary(string tile, int sourceSegments,
        int paths, int triangles) =>
        $"[G06/线条] 瓦片={tile} 原始段={sourceSegments} 折线={paths} " +
        $"绘制三角形={triangles} 中心查询={CenterCount} 中心缺失={CenterMissCount} " +
        $"边沿查询={SideCount} 边沿缺失={SideMissCount} 丢带段={SkippedBandCount} " +
        $"越界={BoundsMissCount} 空桶={BucketMissCount} 无命中={TriangleMissCount} " +
        $"近边恢复={RecoveryCount} 首个中心缺口ENU={_firstMiss}";
}