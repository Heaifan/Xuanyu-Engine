using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

public sealed partial class GeoTerrainRenderSession
{
    private bool IsMiniMapWater(double latitude, double longitude)
    {
        if (_waterAtlas is not { } atlas) return false;
        ReadOnlySpan<short> packed = atlas.PackedSamples.Span;
        int columns = packed[0], rows = packed[1];
        int south = (int)Math.Floor(latitude);
        int west = (int)Math.Floor(longitude);
        int x = west - atlas.WestLongitude, y = south - atlas.SouthLatitude;
        if ((uint)x >= (uint)columns || (uint)y >= (uint)rows) return false;
        int tile = packed[2 + y * columns + x];
        if (tile == 0) return false;
        int eastSample = Math.Clamp((int)((longitude - west) * 3600d), 0, 3600);
        int northSample = Math.Clamp((int)((latitude - south) * 3600d), 0, 3600);
        int sample = northSample * NasademTile.Side + eastSample;
        int words = (NasademTile.SampleCount + 15) / 16;
        int index = 2 + columns * rows + (tile - 1) * words + sample / 16;
        return ((ushort)packed[index] & (1 << (sample % 16))) != 0;
    }
}