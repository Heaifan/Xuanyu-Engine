namespace XuanYu.World.Maps;

public readonly record struct NasademSourceResidency(
    bool IsFileBacked, int Capacity, int ResidentCount, long LoadCount, long EvictedCount)
{
    public long ResidentBytes => ResidentCount * NasademTile.ByteLength;
}