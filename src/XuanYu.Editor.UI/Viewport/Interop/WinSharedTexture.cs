using System.Runtime.InteropServices;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using static Silk.NET.Core.Native.SilkMarshal;

namespace XuanYu.Editor.UI;

internal sealed unsafe class WinSharedTexture : IDisposable
{
    private ComPtr<ID3D11Device> _device;
    private ComPtr<ID3D11DeviceContext> _context;
    private ComPtr<ID3D11Texture2D> _texture;
    public nint Handle { get; private set; }
    public uint Width { get; }
    public uint Height { get; }

    public WinSharedTexture(uint width, uint height)
    {
        Width = width;
        Height = height;
        using var dxgi = new DXGI(DXGI.CreateDefaultContext(["dxgi.dll"]));
        using var d3d = new D3D11(D3D11.CreateDefaultContext(["d3d11.dll"]));
        using var factory = dxgi.CreateDXGIFactory1<IDXGIFactory1>();
        ComPtr<IDXGIAdapter> adapter = default;
        ThrowHResult(factory.EnumAdapters(0, adapter.GetAddressOf()));
        using (adapter)
        {
            var levels = stackalloc D3DFeatureLevel[2]
                { D3DFeatureLevel.Level111, D3DFeatureLevel.Level110 };
            D3DFeatureLevel actual;
            ComPtr<ID3D11Device> device = default;
            ComPtr<ID3D11DeviceContext> context = default;
            ThrowHResult(d3d.CreateDevice(adapter, D3DDriverType.Unknown,
                0, 0, levels, 2, D3D11.SdkVersion,
                device.GetAddressOf(), &actual, context.GetAddressOf()));
            _device = device;
            _context = context;
        }
        var info = new Texture2DDesc
        {
            Width = width, Height = height, ArraySize = 1, MipLevels = 1,
            Format = Format.FormatR8G8B8A8Unorm,
            SampleDesc = new SampleDesc(1, 0),
            Usage = Usage.Default,
            BindFlags = (uint)(BindFlag.RenderTarget | BindFlag.ShaderResource),
            MiscFlags = (uint)(ResourceMiscFlag.SharedNthandle |
                ResourceMiscFlag.SharedKeyedmutex)
        };
        ComPtr<ID3D11Texture2D> texture = default;
        ThrowHResult(_device.CreateTexture2D(&info, (SubresourceData*)null,
            texture.GetAddressOf()));
        _texture = texture;
        using var resource = _texture.QueryInterface<IDXGIResource1>();
        void* shared = null;
        ThrowHResult(resource.CreateSharedHandle((SecurityAttributes*)null, 0x80000001u, (char*)null, &shared));
        Handle = (nint)shared;
    }

    public void Dispose()
    {
        if (Handle != 0) CloseHandle(Handle);
        Handle = 0;
        _texture.Dispose();
        _context.Dispose();
        _device.Dispose();
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool CloseHandle(nint handle);
}