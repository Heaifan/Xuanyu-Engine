namespace XuanYu.World.Maps;

public enum NasademOverviewLevel { Far = 60, Middle = 12 }

internal static class NasademOverviewLayout
{
    public static int Side(NasademOverviewLevel level) => level switch
    {
        NasademOverviewLevel.Far or NasademOverviewLevel.Middle =>
            (NasademTile.Side - 1) / (int)level + 1,
        _ => throw new ArgumentOutOfRangeException(nameof(level))
    };
}