
using System.Numerics;

namespace XuanYu.World.Maps;

public sealed class TerrainHeightField
{
    private readonly float[] _heights;

    public int SamplesX { get; }
    public int SamplesY { get; }
    public float SpacingMeters { get; }
    public float WidthMeters => (SamplesX - 1) * SpacingMeters;
    public float LengthMeters => (SamplesY - 1) * SpacingMeters;
    public int SampleCount => _heights.Length;
    public ReadOnlySpan<float> Samples => _heights;

    public TerrainHeightField(int samplesX, int samplesY, float spacingMeters)
    {
        if (samplesX < 2)
            throw new ArgumentOutOfRangeException(nameof(samplesX));
        if (samplesY < 2)
            throw new ArgumentOutOfRangeException(nameof(samplesY));
        if (!float.IsFinite(spacingMeters) || spacingMeters <= 0f)
            throw new ArgumentOutOfRangeException(nameof(spacingMeters));

        SamplesX = samplesX;
        SamplesY = samplesY;
        SpacingMeters = spacingMeters;
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
            x * SpacingMeters,
            y * SpacingMeters,
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
