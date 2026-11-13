using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace XuanYu.Editor.UI;

public sealed class ViewportCompositionSurface : Control
{
    private CompositionSurfaceVisual? _visual;
    private double _imageAspect;
    public CompositionDrawingSurface? Surface { get; private set; }
    public ICompositionGpuInterop? GpuInterop { get; private set; }
    public event Action<ICompositionGpuInterop>? GpuInteropReady;

    public ViewportCompositionSurface() => ClipToBounds = true;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        var compositor = ElementComposition.GetElementVisual(this)!.Compositor;
        Surface = compositor.CreateDrawingSurface();
        _visual = compositor.CreateSurfaceVisual();
        _visual.Surface = Surface;
        ResizeVisual();
        ElementComposition.SetElementChildVisual(this, _visual);
        _ = InitializeGpuInteropAsync(compositor);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ElementComposition.SetElementChildVisual(this, null);
        if (_visual is not null) _visual.Surface = null;
        _visual = null;
        Surface?.Dispose(); Surface = null;
        GpuInterop = null; base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BoundsProperty)
            ResizeVisual();
    }

    public Task PresentWithKeyedMutexAsync(
        ICompositionImportedGpuImage image, uint acquireKey, uint releaseKey)
    {
        var surface = Surface ?? throw new InvalidOperationException("合成表面尚未创建。");
        return surface.UpdateWithKeyedMutexAsync(image, acquireKey, releaseKey);
    }

    public Task PresentWithSemaphoresAsync(
        ICompositionImportedGpuImage image,
        ICompositionImportedGpuSemaphore waitSemaphore,
        ICompositionImportedGpuSemaphore signalSemaphore)
    {
        var surface = Surface ?? throw new InvalidOperationException("合成表面尚未创建。");
        return surface.UpdateWithSemaphoresAsync(image, waitSemaphore, signalSemaphore);
    }

    public void SetImageSize(uint width, uint height)
    {
        _imageAspect = (double)width / height; ResizeVisual();
    }

    private void ResizeVisual()
    {
        if (_visual is null) return;
        double width = Bounds.Width, height = Bounds.Height;
        if (_imageAspect > 0 && width > height * _imageAspect)
            height = width / _imageAspect;
        else if (_imageAspect > 0)
            width = height * _imageAspect;
        _visual.Size = new Vector2((float)width, (float)height);
        _visual.Offset = new Vector3D((Bounds.Width - width) / 2,
            (Bounds.Height - height) / 2, 0);
    }

    private async Task InitializeGpuInteropAsync(Compositor compositor)
    {
        try
        {
            var interop = await compositor.TryGetCompositionGpuInterop();
            if (interop is null)
            {
                Console.WriteLine("[XYE/E306] Avalonia GPU Interop 不可用");
                return;
            }
            GpuInterop = interop;
            Console.WriteLine("[XYE/E306] Avalonia 合成表面已就绪");
            GpuInteropReady?.Invoke(interop);
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"[XYE/E306] 合成初始化失败：{error}");
        }
    }
}