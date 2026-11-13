using Avalonia.Controls;
using XuanYu.Editor.Pages;
using XuanYu.Editor.Preview;
using XuanYu.Render.Vulkan;

namespace XuanYu.Editor.UI;

public sealed partial class EditorWindow
{
    private Control CreateViewport(VkContext? vk)
    {
        if (OperatingSystem.IsWindows() && vk is not null)
        {
            _sharedViewport = new ViewportSharedHost(vk, _wb);
            _sharedViewport.Clicked += OnViewportClicked;
            Closing += async (_, e) =>
            {
                if (_sharedViewport.IsStopped) return;
                e.Cancel = true;
                await _sharedViewport.StopAsync();
                Close();
            };
            _wb.Log.Write("E3-06D：中央视口使用 Avalonia GPU 合成");
            return _sharedViewport;
        }
        _nativeViewport = new ViewportBox(_wb.ViewportNote);
        _nativeViewport.BindNavigation(_wb);
        _nativeViewport.Clicked += OnViewportClicked;
        if (vk is not null)
        {
            _ = new ViewportSwapchainSession(_nativeViewport, vk, _wb);
            Closing += (_, _) => vk.DetachSurface();
        }
        return _nativeViewport;
    }

    private void OnViewportClicked(int x, int y)
    {
        if (_wb.CurrentId != PageId.World || _wb.IsSimulating)
            return;
        var (width, height) = _sharedViewport?.PixelSize ??
            _nativeViewport?.PixelSize ?? (0u, 0u);
        Guid? hit = PreviewMeshPicker.Pick(_wb.Scene, x, y, width, height);
        if (_wb.Scene.SelectedId == hit)
            return;
        _wb.Scene.Select(hit);
        _wb.Log.Write(hit is null
            ? "视口点击空白，已取消选择"
            : $"视口选中：{_wb.Scene.Selected!.Name}");
    }
}