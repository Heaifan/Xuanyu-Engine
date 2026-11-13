using Avalonia.Controls;
using Avalonia.Platform;

namespace XuanYu.Editor.UI;

public sealed partial class WinSharedProbeWindow
{
    private async Task RenderAsync()
    {
        if (_interop is null) return;
        double scale = TopLevel.GetTopLevel(_scene)?.RenderScaling ?? 1;
        uint width = (uint)Math.Round(_scene.Bounds.Width * scale);
        uint height = (uint)Math.Round(_scene.Bounds.Height * scale);
        if (width < 2 || height < 2) return;
        try
        {
            bool changed = _texture is null ||
                _texture.Width != width || _texture.Height != height;
            if (changed && DateTime.UtcNow - _resizeAt >=
                TimeSpan.FromMilliseconds(80))
                await CreateTargetAsync(width, height);
            if (_texture is null || _probe is null || _image is null) return;
            var draws = _workbench.ViewportDraws(_texture.Width, _texture.Height);
            var probe = _probe;
            var renderWidth = _texture.Width;
            var renderHeight = _texture.Height;
            await Task.Run(() => probe.DrawScene(draws, renderWidth, renderHeight));
            await _scene.PresentWithKeyedMutexAsync(_image, 1, 0);
        }
        catch (Exception error)
        {
            _timer.Stop();
            _status.Text = "共享 3D 帧失败，详见终端";
            Console.Error.WriteLine($"[XYE/E306C] {error}");
        }
    }

    private async Task CreateTargetAsync(uint width, uint height)
    {
        await ReleaseResourcesAsync();
        _texture = new WinSharedTexture(width, height);
        _probe = _vk.CreateSharedColorProbe(_texture.Handle, width, height);
        _image = _interop!.ImportImage(
            new PlatformHandle(_texture.Handle, "D3D11TextureNtHandle"),
            new PlatformGraphicsExternalImageProperties
            {
                Width = (int)width, Height = (int)height,
                Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                TopLeftOrigin = true
            });
        _status.Text = $"Vulkan 3D + MSAA 4x + Avalonia：{width}×{height}";
        Console.WriteLine($"[XYE/E306C] 真实 3D 共享目标创建：{width}×{height}");
    }

    private async Task ReleaseResourcesAsync()
    {
        if (_image is not null) await _image.DisposeAsync();
        _image = null;
        _probe?.Dispose();
        _probe = null;
        _texture?.Dispose();
        _texture = null;
    }
}