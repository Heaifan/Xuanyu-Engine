
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;

namespace XuanYu.Editor.UI;

internal static class ViewportOverlayProbe
{
    public static Control Wrap(Control nativeHost)
    {
        var popup = new Popup
        {
            PlacementTarget = nativeHost,
            Placement = PlacementMode.AnchorAndGravity,
            PlacementAnchor = PopupAnchor.TopRight,
            PlacementGravity = PopupGravity.BottomLeft,
            HorizontalOffset = -12,
            VerticalOffset = 12,
            ShouldUseOverlayLayer = false,
            TakesFocusFromNativeControl = false,
            IsLightDismissEnabled = false,
            WindowManagerAddShadowHint = false,
            Child = new ViewportNavigationPanel()
        };

        var layout = new Grid();
        layout.Children.Add(nativeHost);
        layout.Children.Add(popup);

        nativeHost.AttachedToVisualTree += (_, _) =>
            Dispatcher.UIThread.Post(() =>
            {
                popup.IsOpen = true;
                _ = ProbeAsync(layout);
            }, DispatcherPriority.Loaded);

        nativeHost.DetachedFromVisualTree += (_, _) =>
            popup.IsOpen = false;

        return layout;
    }

    private static async Task ProbeAsync(Control anchor)
    {
        try
        {
            var visual = ElementComposition.GetElementVisual(anchor);
            if (visual is null)
            {
                Console.WriteLine("[XYE/E306] 无 CompositionVisual");
                return;
            }

            var interop = await visual.Compositor
                .TryGetCompositionGpuInterop();

            if (interop is null)
            {
                Console.WriteLine("[XYE/E306] GPU Interop 不可用");
                return;
            }

            Console.WriteLine("[XYE/E306] GPU Interop 可用");
            Console.WriteLine($"[XYE/E306] IsLost: {interop.IsLost}");
            Console.WriteLine($"[XYE/E306] DeviceLuid: " +
                $"{Convert.ToHexString(interop.DeviceLuid ?? [])}");
            Console.WriteLine($"[XYE/E306] DeviceUuid: " +
                $"{Convert.ToHexString(interop.DeviceUuid ?? [])}");

            foreach (string type in interop.SupportedImageHandleTypes)
            {
                var sync = interop.GetSynchronizationCapabilities(type);
                Console.WriteLine($"[XYE/E306] Image: {type}");
                Console.WriteLine($"[XYE/E306] Sync: {sync}");
            }

            foreach (string type in interop.SupportedSemaphoreTypes)
                Console.WriteLine($"[XYE/E306] Semaphore: {type}");
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(
                $"[XYE/E306] 探测失败：{error}");
        }
    }
}
