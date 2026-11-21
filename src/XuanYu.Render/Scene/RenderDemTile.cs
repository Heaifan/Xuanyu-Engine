namespace XuanYu.Render;

// 只描述已解码的原始高程；不管理加载、缓存与 GPU 生命周期。
public readonly record struct RenderDemTile(
    int SouthLatitude,
    int WestLongitude,
    ReadOnlyMemory<short> SamplesNorthToSouth)
{
    public const int Side = 3601;
    public const short NoData = short.MinValue;
}