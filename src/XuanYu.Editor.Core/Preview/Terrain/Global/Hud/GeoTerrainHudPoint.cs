namespace XuanYu.Editor.Preview.Terrain;

public readonly record struct GeoTerrainHudPoint(
    double Latitude, double Longitude, double ElevationMeters, string TileId);