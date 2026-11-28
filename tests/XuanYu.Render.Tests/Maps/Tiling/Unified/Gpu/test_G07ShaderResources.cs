using XuanYu.Render.Vulkan;
using Xunit;

namespace XuanYu.World.Tests.Maps;

public sealed class test_G07ShaderResources
{
    [Theory]
    [InlineData("g07.terrain.vert.spv")]
    [InlineData("g07.terrain.frag.spv")]
    public void TerrainShadersAreEmbeddedSpirv(string name)
    {
        using var stream = typeof(VkContext).Assembly.GetManifestResourceStream(name);
        Assert.NotNull(stream);
        using var reader = new BinaryReader(stream!);
        Assert.Equal(0x07230203u, reader.ReadUInt32());
        Assert.True(stream!.Length > 20 && stream.Length % 4 == 0);
    }
}
