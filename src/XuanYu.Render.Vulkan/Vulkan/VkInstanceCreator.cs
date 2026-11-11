
using Silk.NET.Vulkan;

namespace XuanYu.Render.Vulkan;

internal static unsafe class VkInstanceCreator
{
    public static Instance Create(Vk vk)
    {
        byte[] appName = "XuanYuEngine\0"u8.ToArray();
        byte[] engineName = "XuanYu\0"u8.ToArray();
        byte[] surfaceName = "VK_KHR_surface\0"u8.ToArray();
        byte[] win32Name = "VK_KHR_win32_surface\0"u8.ToArray();

        fixed (byte* app = appName)
        fixed (byte* engine = engineName)
        fixed (byte* surface = surfaceName)
        fixed (byte* win32 = win32Name)
        {
            byte** extensions = stackalloc byte*[2] { surface, win32 };
            bool useWin32 = OperatingSystem.IsWindows();
            var application = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = app,
                PEngineName = engine,
                ApiVersion = Vk.Version13
            };
            var createInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &application,
                EnabledExtensionCount = useWin32 ? 2u : 0u,
                PpEnabledExtensionNames = useWin32 ? extensions : null
            };
            Instance instance = default;
            Result result = vk.CreateInstance(&createInfo, null, &instance);
            if (result != Result.Success)
                throw new InvalidOperationException($"Vulkan Instance 创建失败：{result}");
            return instance;
        }
    }
}
