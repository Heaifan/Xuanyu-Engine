using Xunit;
using XuanYu.Editor.UI.Right;

namespace XuanYu.World.Tests;

public sealed class test_TransformNumber
{
    [Theory]
    [InlineData("2.5", 2.5f)]
    [InlineData("-1.25", -1.25f)]
    [InlineData("0", 0f)]
    [InlineData("1e2", 100f)]
    public void TryParse_ValidFloat_ShouldAccept(string text, float expected)
    {
        Assert.True(TransformNumber.TryParse(text, out float value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1,5")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("1e99")]
    public void TryParse_InvalidFloat_ShouldReject(string text)
    {
        Assert.False(TransformNumber.TryParse(text, out _));
    }

    [Fact]
    public void Step_ShouldMoveInBothDirections()
    {
        Assert.Equal(2.5f, TransformNumber.Step(2f, 0.1f, 5f));
        Assert.Equal(1.5f, TransformNumber.Step(2f, 0.1f, -5f));
    }

    [Theory]
    [InlineData(25f, 2.5f)]
    [InlineData(-25f, -2.5f)]
    [InlineData(3f, 0f)]
    [InlineData(-3f, 0f)]
    public void Scrub_UsesPointerDeltaAndStep(float pixels, float expected)
    {
        Assert.Equal(expected, TransformNumber.Scrub(0f, 0.5f, pixels));
    }

    [Fact]
    public void Scrub_UsesOriginalValueWithoutAccumulatingError()
    {
        Assert.Equal(2.5f, TransformNumber.Scrub(2f, 0.1f, 25f));
        Assert.Equal(1.5f, TransformNumber.Scrub(2f, 0.1f, -25f));
    }
}