using System.Numerics;
using Xunit;
using XuanYu.World;

namespace XuanYu.World.Tests;

public sealed class test_Transform
{
    [Fact]
    public void Identity_ShouldHaveExpectedValues()
    {
        var transform = Transform.Identity;

        Assert.Equal(Vector3.Zero, transform.Position);
        Assert.Equal(Quaternion.Identity, transform.Rotation);
        Assert.Equal(Vector3.One, transform.Scale);
    }

    [Fact]
    public void With_ShouldPreserveOriginalValue()
    {
        var original = Transform.Identity;
        var moved = original with
        {
            Position = new Vector3(2.5f, 0f, 1f)
        };

        Assert.Equal(Vector3.Zero, original.Position);
        Assert.Equal(2.5f, moved.Position.X);
        Assert.Equal(original.Rotation, moved.Rotation);
        Assert.Equal(original.Scale, moved.Scale);
    }
}