using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace scpcb.Graphics; 

public class GraphicsResources : Disposable {
    public GraphicsDevice GraphicsDevice { get; }

    public ShaderCache ShaderCache { get; }
    public TextureCache TextureCache { get; }

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
            SwapchainDepthFormat = PixelFormat.R16_UNorm,
        }, out var window, out var gfx);
        window.CursorVisible = false;

        Window = window;
        GraphicsDevice = gfx;
        ShaderCache = new(this);
        TextureCache = new(gfx);

        gfx.GetOpenGLInfo(out var info);
        _preferredShaderFileExtension = gfx.BackendType switch {
            GraphicsBackend.Direct3D11 => new [] { "hlsl.bytes", "hlsl" },
            GraphicsBackend.OpenGL => new[] { "330.glsl" }, //TODO: Reinvestigate?
                //GetOpenGLShaderVersion() >= 450
                //? "450.glsl" + (/*info.Extensions.Contains("GL_ARB_gl_spirv") ? ".spv" :*/ "")
                //: "330.glsl",
            GraphicsBackend.OpenGLES => new[] { "300.glsles" },
            GraphicsBackend.Vulkan => new [] { "450.glsl.spv" },
            GraphicsBackend.Metal => new [] { "metallib" },
            _ => throw new NotImplementedException(),
        };

        int GetOpenGLShaderVersion()
            => int.Parse(info.ShadingLanguageVersion[..info.ShadingLanguageVersion.IndexOf(' ')].Replace(".", ""));
    }

    private readonly string[] _preferredShaderFileExtension;
    // TODO: We can probably support HLSL here as well.
    // Can we support other GLSL versions?
    private static readonly string[] _fallbackShaderFileExtension = { "450.glsl.spv", "330.glsl" };

    /// <summary>
    /// Gets the best supported extension for the given shader.
    /// Vertex and fragment shader are guaranteed to exist for the extension.
    /// </summary>
    /// <param name="shaderName">E.g. "MyShader", "MyShader-vertex.hlsl" etc. will be checked</param>
    /// <param name="needsSpirVBridge">Whether transpilation to SPIR-V is required (via veldrid-spirv)</param>
    /// <returns></returns>
    public string GetShaderFileExtension(string shaderName, out bool needsSpirVBridge) {
        var t = _preferredShaderFileExtension
            .Select(x => (Extension: x, RequiresSpirVBridge: false))
            .Concat(_fallbackShaderFileExtension.Select(x => (Extension: x, RequiresSpirVBridge: true)))
            .FirstOrDefault(x
                => File.Exists($"{shaderName}-vertex.{x.Extension}")
                && File.Exists($"{shaderName}-fragment.{x.Extension}"));

        if (t == default) {
            throw new($"Shader {shaderName} not found with suitable extension!");
        }

        needsSpirVBridge = t.RequiresSpirVBridge;
        return t.Extension;
    }

    protected override void DisposeImpl() {
        ShaderCache.Dispose();
        GraphicsDevice.Dispose();
    }
}
