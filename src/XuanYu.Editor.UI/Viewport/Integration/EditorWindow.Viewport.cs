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
            _sharedViewport.G07TransformCommitted += _inspector.RefreshTransform;
            _sharedViewport.G07PositionChanged += _inspector.RefreshGeographicPosition;
            bool closing = false, released = false;
            Closing += async (_, e) =>
            {
                if (released) return;
                e.Cancel = true;
                if (closing) return;
                closing = true;
                Console.WriteLine($"[GPU/退出] {Environment.TickCount64} 收到关闭请求");
                await _sharedViewport.StopAsync();
                released = true;
                Avalonia.Threading.Dispatcher.UIThread.Post(Close);
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

    private void CreateWorldCube()
    {
        if (_wb.IsSimulating) return;
        if (!_wb.IsGeoTerrainPreview) _wb.Scene.CreateCube();
        else if (_sharedViewport is { } view) view.CreateG07CubeAtTarget();
        else _wb.Log.Warn("当前视口尚未接入全球实体创建。");
    }

    private void OnViewportClicked(int x, int y)
    {
        if (_wb.CurrentId != PageId.World || _wb.IsSimulating) return;
        if (_wb.IsGeoTerrainPreview)
        {
            _sharedViewport?.QueueG07EntityPick(x, y);
            return;
        }
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