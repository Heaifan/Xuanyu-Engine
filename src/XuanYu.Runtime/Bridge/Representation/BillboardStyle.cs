namespace XuanYu.Runtime;

public readonly record struct BillboardStyle(float Size, bool FixedPixels)
{
    public static BillboardStyle Default => new(64, true);
    public static BillboardStyle Point => new(10, true);
}