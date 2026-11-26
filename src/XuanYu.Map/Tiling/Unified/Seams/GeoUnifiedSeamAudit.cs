namespace XuanYu.World.Maps;

public readonly record struct GeoUnifiedSeamReport(
    string First, string Second, int BothValid, int Conflicts,
    int MaxDifferenceMeters, int FirstMissing, int SecondMissing);

public static class GeoUnifiedSeamAudit
{
    public static GeoUnifiedSeamReport Compare(NasademTile first, NasademTile second)
    {
        int dx = second.WestLongitude - first.WestLongitude;
        if (dx == -359) dx = 1;
        if (dx == 359) dx = -1;
        int dy = second.SouthLatitude - first.SouthLatitude;
        if (Math.Abs(dx) + Math.Abs(dy) != 1) throw new ArgumentException("瓦片不共边。");
        int valid = 0, conflicts = 0, max = 0, missingFirst = 0, missingSecond = 0;
        for (int i = 0; i < NasademTile.Side; i++)
        {
            short? a = dx != 0 ? first.HeightAt(dx > 0 ? 3600 : 0, i) :
                first.HeightAt(i, dy > 0 ? 3600 : 0);
            short? b = dx != 0 ? second.HeightAt(dx > 0 ? 0 : 3600, i) :
                second.HeightAt(i, dy > 0 ? 0 : 3600);
            if (a is null) missingFirst++;
            if (b is null) missingSecond++;
            if (a is null || b is null) continue;
            valid++;
            int difference = Math.Abs((int)a.Value - b.Value);
            if (difference != 0) conflicts++;
            max = Math.Max(max, difference);
        }
        return new(first.TileId, second.TileId, valid, conflicts, max, missingFirst, missingSecond);
    }
}
