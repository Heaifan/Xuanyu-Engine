
using System.Numerics;

namespace XuanYu.World.Maps;

public sealed class TerrainHeightField
{
    private readonly float[] _heights;

    public int SamplesX { get; }
    public int SamplesY { get; }
    public float SpacingXMeters { get; }
    public float SpacingYMeters { get; }
    public float SpacingMeters => SpacingXMeters;
    public float WidthMeters => (SamplesX - 1) * SpacingXMeters;
    public float LengthMeters => (SamplesY - 1) * SpacingYMeters;
    public int SampleCount => _heights.Length;
    public ReadOnlySpan<float> Samples => _heights;

    public TerrainHeightField(int samplesX, int samplesY, float spacingMeters)
        : this(samplesX, samplesY, spacingMeters, spacingMeters) { }

    public TerrainHeightField(int samplesX, int samplesY,
        float spacingXMeters, float spacingYMeters)
    {
        if (samplesX < 2)
            throw new ArgumentOutOfRangeException(nameof(samplesX));
        if (samplesY < 2)
            throw new ArgumentOutOfRangeException(nameof(samplesY));
        if (!float.IsFinite(spacingXMeters) || spacingXMeters <= 0f)
            throw new ArgumentOutOfRangeException(nameof(spacingXMeters));
        if (!float.IsFinite(spacingYMeters) || spacingYMeters <= 0f)
            throw new ArgumentOutOfRangeException(nameof(spacingYMeters));

        SamplesX = samplesX;
        SamplesY = samplesY;
        SpacingXMeters = spacingXMeters;
        SpacingYMeters = spacingYMeters;
        _heights = new float[checked(samplesX * samplesY)];
    }

    public float GetHeight(int x, int y) => _heights[IndexOf(x, y)];

    public void SetHeight(int x, int y, float height)
    {
        if (!float.IsFinite(height))
            throw new ArgumentOutOfRangeException(nameof(height));

        _heights[IndexOf(x, y)] = height;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        int index = IndexOf(x, y);
        return new Vector3(
            x * SpacingXMeters,
            y * SpacingYMeters,
            _heights[index]);
    }

    private int IndexOf(int x, int y)
    {
        if ((uint)x >= (uint)SamplesX)
            throw new ArgumentOutOfRangeException(nameof(x));
        if ((uint)y >= (uint)SamplesY)
            throw new ArgumentOutOfRangeException(nameof(y));

        return y * SamplesX + x;
    }
}
