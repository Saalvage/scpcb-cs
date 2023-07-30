using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace scpcb.Graphics; 

public class GraphicsResources : Disposable {
    public GraphicsDevice GraphicsDevice { get; }

    public ShaderCache ShaderCache { get; }
    public TextureCache TextureCache { get; }

    public Sdl2Window Window { get; }

    public string ShaderFileExtension { get; }

    public GraphicsResources(int width, int height) {
        VeldridStartup.CreateWindowAndGraphicsDevice(new() {
            WindowWidth = width,
            WindowHeight = height,
            X = 100,
            Y = 100,
            WindowTitle = "SCP-087-B",
        }, new() {
            Debug = true,
            SwapchainDepthFormat = PixelFormat.R16_UNorm,
        }, out var window, out var gfx);
        window.CursorVisible = false;

        Window = window;
        GraphicsDevice = gfx;
        ShaderCache = new(this);
        TextureCache = new(gfx);

        gfx.GetOpenGLInfo(out var info);
        ShaderFileExtension = gfx.BackendType switch {
            GraphicsBackend.OpenGL => "330.glsl", //TODO: Reinvestigate?
            GraphicsBackend.Direct3D11 => "hlsl.bytes",
                //GetOpenGLShaderVersion() >= 450
                //? "450.glsl" + (/*info.Extensions.Contains("GL_ARB_gl_spirv") ? ".spv" :*/ "")
                //: "330.glsl",
            GraphicsBackend.OpenGLES => "300.glsles",
            GraphicsBackend.Vulkan => "450.glsl.spv",
            GraphicsBackend.Metal => "metallib",
            _ => throw new NotImplementedException(),
        };

        int GetOpenGLShaderVersion()
            => int.Parse(info.ShadingLanguageVersion[..info.ShadingLanguageVersion.IndexOf(' ')].Replace(".", ""));
    }

    public string GetShaderFile(string shaderName, out bool needsToUseSpirVBridge) {
        needsToUseSpirVBridge = false;

        var preferredFile = _preferredShaderFileExtension
            .Select(x => shaderName + x)
            .FirstOrDefault(File.Exists);

        if (preferredFile != null) {
            return preferredFile;
        }

        needsToUseSpirVBridge = true;

        var fallbackFile = _fallbackShaderFileExtension
            .Select(x => shaderName + x)
            .FirstOrDefault(File.Exists);
    }

    protected override void DisposeImpl() {
        ShaderCache.Dispose();
        GraphicsDevice.Dispose();
    }
}
