using System.Numerics;
using XuanYu.Core.Geodesy;
using XuanYu.World.Maps;

namespace XuanYu.Editor.Preview.Terrain;

internal sealed class GeoTerrainFootprintEdge
{
    public double[] Knots { get; }
    private readonly GeoTerrainEdgeVertex?[] _vertices;

    public GeoTerrainFootprintEdge(double[] knots, GeoTerrainEdgeVertex?[] vertices)
    {
        Knots = knots;
        _vertices = vertices;
    }

    public GeoTerrainEdgeVertex? At(double value)
    {
        int i = Array.BinarySearch(Knots, value);
        if (i >= 0) return _vertices[i];
        i = Math.Clamp(~i - 1, 0, Knots.Length - 2);
        if (_vertices[i] is not { } a || _vertices[i + 1] is not { } b) return null;
        float t = (float)Math.Clamp((value - Knots[i]) / (Knots[i + 1] - Knots[i]), 0d, 1d);
        return GeoTerrainEdgeVertex.Lerp(a, b, t);
    }

    public bool Matches(GeoTerrainFootprintEdge other) =>
        ReferenceEquals(this, other) || Knots.SequenceEqual(other.Knots) &&
        _vertices.SequenceEqual(other._vertices);

    public static GeoTerrainEdgeVertex? Sample(NasademElevationSource source,
        EnuFrame anchor, double latitude, double longitude)
    {
        if (source.Sample(latitude, longitude) is not { } height) return null;
        EnuPosition p = anchor.ToEnu(Wgs84.ToEcef(new GeoCoordinate(latitude, longitude, 0d)));
        return GeoTerrainEdgeVertex.FromHeight(
            new Vector3((float)p.East, (float)p.North, (float)(p.Up + height)), height);
    }
}