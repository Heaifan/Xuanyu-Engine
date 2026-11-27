using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private async Task<(WinSharedTexture Texture, VkSharedColorProbe Probe,
        ICompositionImportedGpuImage Image)> CreateTargetAsync(uint width, uint height)
    {
        var created = await Task.Run(() =>
        {
            var texture = new WinSharedTexture(width, height);
            try { return (Texture: texture,
                Probe: _vk.CreateSharedColorProbe(texture.Handle, width, height)); }
            catch { texture.Dispose(); throw; }
        });
        try
        {
            var image = _interop!.ImportImage(
                new PlatformHandle(created.Texture.Handle, "D3D11TextureNtHandle"),
                new PlatformGraphicsExternalImageProperties
                {
                    Width = (int)width, Height = (int)height,
                    Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                    TopLeftOrigin = true
                });
            return (created.Texture, created.Probe, image);
        }
        catch { created.Probe.Dispose(); created.Texture.Dispose(); throw; }
    }

    private static async Task ReleaseTargetAsync(ICompositionImportedGpuImage? image,
        VkSharedColorProbe? probe, WinSharedTexture? texture)
    {
        Console.WriteLine($"[G06/退出] {Environment.TickCount64} 开始释放合成图像");
        if (image is not null) await image.DisposeAsync();
        Console.WriteLine($"[G06/退出] {Environment.TickCount64} 合成图像已释放，开始释放 Vulkan 目标");
        await Task.Run(() => { probe?.Dispose(); texture?.Dispose(); });
        Console.WriteLine($"[G06/退出] {Environment.TickCount64} Vulkan 与 D3D 目标已释放");
    }
}
