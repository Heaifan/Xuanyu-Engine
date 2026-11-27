using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal sealed unsafe class VkTriangleShaders : IDisposable
{
    private readonly Vk _vk;
    private readonly Device _device;

    public ShaderModule Vertex { get; }
    public ShaderModule Fragment { get; }

    public VkTriangleShaders(Vk vk, Device device,
        string vertex = "triangle.vert.spv", string fragment = "triangle.frag.spv")
    {
        _vk = vk;
        _device = device;
        Vertex = Create(vertex);
        try
        {
            Fragment = Create(fragment);
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    private ShaderModule Create(string resource)
    {
        using Stream stream = typeof(VkTriangleShaders).Assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"缺少 Vulkan shader：{resource}");
        byte[] bytes = new byte[stream.Length];
        stream.ReadExactly(bytes);
        uint[] words = new uint[bytes.Length / sizeof(uint)];
        System.Buffer.BlockCopy(bytes, 0, words, 0, bytes.Length);

        fixed (uint* code = words)
        {
            var info = new ShaderModuleCreateInfo
            {
                SType = StructureType.ShaderModuleCreateInfo,
                CodeSize = (nuint)bytes.Length,
                PCode = code
            };

            Result result = _vk.CreateShaderModule(
                _device, &info, null, out var module);
            if (result != Result.Success)
                throw new InvalidOperationException($"创建 ShaderModule 失败：{result}");
            return module;
        }
    }

    public void Dispose()
    {
        if (Fragment.Handle != 0)
            _vk.DestroyShaderModule(_device, Fragment, null);
        if (Vertex.Handle != 0)
            _vk.DestroyShaderModule(_device, Vertex, null);
    }
}