namespace XuanYu.Render;

public sealed record RenderWaterAtlas(
    int SouthLatitude,
    int WestLongitude,
    int Side,
    ReadOnlyMemory<short> PackedSamples,
    int LoadedCount);