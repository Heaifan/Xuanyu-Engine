using Xunit;
using XuanYu.Editor.Camera;

namespace XuanYu.World.Tests;

public sealed class test_GameCameraKey
{
    [Theory]
    [InlineData(0x57u, GameCameraKey.Forward)]
    [InlineData(0x77u, GameCameraKey.Forward)]
    [InlineData(0x53u, GameCameraKey.Backward)]
    [InlineData(0x41u, GameCameraKey.Left)]
    [InlineData(0x44u, GameCameraKey.Right)]
    [InlineData(0x20u, GameCameraKey.None)]
    public void KeySymbols_ShouldMapToCameraActions(uint code, GameCameraKey key)
    {
        Assert.Equal(key, GameCameraKeyMap.FromKeySymbol(code));
    }
}