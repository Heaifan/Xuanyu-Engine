using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private static readonly TimeSpan ResizeInterval = TimeSpan.FromMilliseconds(80);
    private async Task RenderAsync()
    {
        if (_interop is null || _stopped) return;
        var (requestedWidth, requestedHeight) = PixelSize;
        if (requestedWidth < 2 || requestedHeight < 2) return;
        long startedAt = Environment.TickCount64;
        (WinSharedTexture Texture, VkSharedColorProbe Probe,
            ICompositionImportedGpuImage Image)? next = null;
        try
        {
            bool changed = _texture is null || _texture.Width != requestedWidth ||
                _texture.Height != requestedHeight;
            if (changed && (_texture is null ||
                DateTime.UtcNow - _lastTargetAt >= ResizeInterval))
                next = await CreateTargetAsync(requestedWidth, requestedHeight);

            var texture = next?.Texture ?? _texture;
            if (texture is null) return;
            var probe = next?.Probe ?? _probe!;
            var image = next?.Image ?? _image!;
            uint width = texture.Width, height = texture.Height;
            var draws = _wb.ViewportDraws(width, height);
            await Task.Run(() => probe.DrawScene(draws, width, height));
            await _scene.PresentWithKeyedMutexAsync(image, 1, 0);
            _scene.SetImageSize(width, height);

            if (next is { } ready)
            {
                var previous = (_image, _probe, _texture);
                _image = ready.Image; _probe = ready.Probe; _texture = ready.Texture;
                next = null;
                _lastTargetAt = DateTime.UtcNow;
                _wb.Log.Write($"E3-06D GPU 目标：{width}×{height} | 重建首帧：{Environment.TickCount64 - startedAt}ms");
                await ReleaseTargetAsync(previous.Item1, previous.Item2, previous.Item3);
            }
        }
        catch (Exception error)
        {
            if (next is { } failed)
                await ReleaseTargetAsync(failed.Image, failed.Probe, failed.Texture);
            _timer.Stop();
            _wb.Log.Write($"E3-06D 共享视口帧失败：{error.Message}");
            Console.Error.WriteLine($"[XYE/E306D] {error}");
        }
    }

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
        catch
        {
            created.Probe.Dispose(); created.Texture.Dispose();
            throw;
        }
    }

    private static async Task ReleaseTargetAsync(
        ICompositionImportedGpuImage? image, VkSharedColorProbe? probe,
        WinSharedTexture? texture)
    {
        if (image is not null) await image.DisposeAsync();
        await Task.Run(() => { probe?.Dispose(); texture?.Dispose(); });
    }

    public async Task StopAsync()
    {
        _stopped = true; _timer.Stop();
        await _frame;
        await ReleaseTargetAsync(_image, _probe, _texture);
        _image = null; _probe = null; _texture = null;
    }
}