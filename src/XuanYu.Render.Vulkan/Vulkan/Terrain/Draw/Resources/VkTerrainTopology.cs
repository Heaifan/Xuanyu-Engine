using System.Runtime.InteropServices;
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTerrainTopology : IDisposable
{
    private readonly VkTerrainBuffer _buffer;
    private readonly (uint First, uint Count)[] _ranges = new (uint, uint)[16];

    public VkTerrainTopology(Vk vk, PhysicalDevice gpu, Device device, IReadOnlyList<int[]> topology)
    {
        var packed = new List<ushort>();
        for (int mask = 0; mask < 16; mask++)
        {
            var indices = topology[mask];
            _ranges[mask] = ((uint)packed.Count, (uint)indices.Length);
            foreach (int index in indices) packed.Add((ushort)index);
        }
        var bytes = MemoryMarshal.AsBytes(CollectionsMarshal.AsSpan(packed));
        _buffer = new VkTerrainBuffer(vk, gpu, device, (ulong)bytes.Length, BufferUsageFlags.IndexBufferBit);
        try { _buffer.Write(bytes); }
        catch { _buffer.Dispose(); throw; }
    }

    public void Draw(Vk vk, CommandBuffer command, int mask)
    {
        var range = _ranges[mask];
        vk.CmdBindIndexBuffer(command, _buffer.Handle, 0, IndexType.Uint16);
        vk.CmdDrawIndexed(command, range.Count, 1, range.First, 0, 0);
    }

    public void Dispose() => _buffer.Dispose();
}