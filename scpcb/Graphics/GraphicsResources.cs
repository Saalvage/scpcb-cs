using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Vulkan.Xlib;

namespace scpcb.Graphics; 

public class GraphicsResources : Disposable {
    public GraphicsDevice GraphicsDevice { get; }

    public ShaderCache ShaderCache { get; }

    public Sdl2Window Window { get; }

    public GraphicsResources(int width, int height) {
        VeldridStartup.CreateWindowAndGraphicsDevice(new() {
            WindowWidth = width,
            WindowHeight = height,
            X = 100,
            Y = 100,
            WindowTitle = "SCP-087-B",
        }, new() {
            Debug = true,
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
            SwapchainDepthFormat = PixelFormat.R32_Float,
        }, out var window, out var gfx);
        window.CursorVisible = false;

        Window = window;
        GraphicsDevice = gfx;
        ShaderCache = new(gfx);
    }

    protected override void DisposeImpl() {
        ShaderCache.Dispose();
        GraphicsDevice.Dispose();
    }
}
