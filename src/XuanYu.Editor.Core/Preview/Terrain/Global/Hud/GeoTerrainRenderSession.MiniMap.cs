using XuanYu.Render;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private GeoTerrainMiniMapData? _miniMap;
    private RenderWaterAtlas? _miniMapWater;

    public GeoTerrainMiniMapData? MiniMap()
    {
        if (!_source.HasOverview) return null;
        if (_miniMap is not null && ReferenceEquals(_miniMapWater, _waterAtlas))
            return _miniMap;
        const int columns = 48, rows = 64;
        byte[] cells = new byte[columns * rows];
        var coarse = _source.CoarseFor(9);
        double south = _coverage.SouthLatitudeDegrees;
        double west = _coverage.WestLongitudeDegrees;
        double north = _coverage.NorthLatitudeDegrees;
        double east = _coverage.EastLongitudeDegrees;
        for (int y = 0; y < rows; y++)
        {
            double lat = north - (y + 0.5d) * (north - south) / rows;
            for (int x = 0; x < columns; x++)
            {
                double lon = west + (x + 0.5d) * (east - west) / columns;
                double? h = coarse.Sample(lat, lon);
                cells[y * columns + x] = h is not { } height ? (byte)0 :
                    IsMiniMapWater(lat, lon) ? (byte)1 : HeightBand(height);
            }
        }
        _miniMapWater = _waterAtlas;
        return _miniMap = new(south, west, north, east, columns, rows, cells);
    }

    private static byte HeightBand(double height) => height switch
    {
        < 100d => 2, < 250d => 3, < 450d => 4,
        < 650d => 5, < 900d => 6, < 1300d => 7,
        < 1800d => 8, < 2400d => 9, < 3000d => 10,
        < 3600d => 11, < 4200d => 12, _ => 13
    };
}