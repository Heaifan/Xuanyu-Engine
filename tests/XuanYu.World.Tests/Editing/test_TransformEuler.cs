using System.Numerics;
using Xunit;
using XuanYu.Editor.UI.Right;

namespace XuanYu.World.Tests;

public sealed class test_TransformEuler
{
    [Theory]
    [InlineData(0f, 0f, 0f)]
    [InlineData(20f, -30f, 45f)]
    [InlineData(-40f, 15f, 95f)]
    public void Rotation_ShouldRoundTripDegrees(float x, float y, float z)
    {
        Vector3 input = new(x, y, z);
        Quaternion rotation = TransformEuler.FromDegrees(input);
        Vector3 result = TransformEuler.ToDegrees(rotation);

        Assert.InRange(Vector3.Distance(input, result), 0f, 0.01f);
    }

    [Theory]
    [InlineData(90f)]
    [InlineData(-90f)]
    public void NearGimbalLock_ShouldKeepEquivalentOrientation(float y)
    {
        Quaternion original = TransformEuler.FromDegrees(new Vector3(30f, y, 20f));
        Quaternion restored = TransformEuler.FromDegrees(
            TransformEuler.ToDegrees(original));

        Assert.InRange(MathF.Abs(Quaternion.Dot(original, restored)),
            0.999f, 1.001f);
    }
}