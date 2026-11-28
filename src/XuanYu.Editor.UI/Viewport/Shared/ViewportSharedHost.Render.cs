using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using System.Diagnostics;
using XuanYu.Render;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed partial class ViewportSharedHost
{
    private static readonly TimeSpan ResizeInterval = TimeSpan.FromMilliseconds(80);
    private async Task RenderAsync()
    {
        if (_interop is null || _stopped) return;
        bool profile = Environment.GetEnvironmentVariable("XYE_G06_PERF") == "1";
        long frameStarted = profile ? Stopwatch.GetTimestamp() : 0;
        double intervalMs = BeginSharedFrameProfile(frameStarted, profile);
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
            long stage = profile ? Stopwatch.GetTimestamp() : 0;
            var geo = _wb.G07PreviewSource;
            RenderDraw[] draws = geo is null
                ? PrepareContourLabels(_wb.ViewportDraws(width, height), width, height) : [];
            long prepared = profile ? Stopwatch.GetTimestamp() : 0;
            var brushFrame = CaptureTerrainBrushFrame(width, height);
            long brushed = profile ? Stopwatch.GetTimestamp() : 0;
            string gpuProfile = geo is null
                ? await Task.Run(() => probe.DrawScene(draws, width, height))
                : await DrawG07FrameAsync(probe, geo, width, height);
            long drawn = profile ? Stopwatch.GetTimestamp() : 0;
            await _scene.PresentWithKeyedMutexAsync(image, 1, 0);
            long presented = profile ? Stopwatch.GetTimestamp() : 0;
            _scene.SetImageSize(width, height);
            if (geo is not null) _wb.CompleteG07Import(geo);
            DisplayTerrainBrushFrame(brushFrame);
            DisplayContourLabels();
            UpdateGeoHudScale();
            UpdateGeoMiniMap();
            if (geo is not null)
            {
                DisplayG07HudHover(geo, width, height);
                DisplayG07EntityPick(geo, width, height);
            }
            DisplayG07Gizmo(geo is not null);
            long uiUpdated = profile ? Stopwatch.GetTimestamp() : 0;

            if (next is { } ready)
            {
                var previous = (_image, _probe, _texture);
                _image = ready.Image; _probe = ready.Probe; _texture = ready.Texture;
                next = null;
                _lastTargetAt = DateTime.UtcNow;
                _wb.Log.Write($"E3-06D GPU 目标：{width}×{height} | 重建首帧：{Environment.TickCount64 - startedAt}ms");
                await ReleaseTargetAsync(previous.Item1, previous.Item2, previous.Item3);
            }
            ReportSharedFrameProfile(profile, intervalMs, frameStarted,
                prepared, stage, brushed, drawn, presented, uiUpdated, gpuProfile);
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
}