using scpcb.Graphics.Primitives;
using scpcb.Graphics.Utility;
using scpcb.Map;
using scpcb.Map.RoomProviders;
using scpcb.Physics;
using scpcb.Utility;
using Serilog;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace scpcb.Graphics;

public class GraphicsResources : Disposable {
    public GraphicsDevice GraphicsDevice { get; }

    public ShaderCache ShaderCache { get; }
    public TextureCache TextureCache { get; }

    public ICBTexture MissingTexture { get; }

    public Sdl2Window Window { get; }

    public RenderTarget MainTarget { get; }

    public bool Debug { get; }

    private readonly RoomProviderCollector _roomProviderCollector = new();

    public GraphicsResources(int width, int height, bool debug =
#if DEBUG
        true
#else
        false
#endif
        ) {
        Debug = debug;

        Window = VeldridStartup.CreateWindow(new() {
            WindowWidth = width,
            WindowHeight = height,
            X = 300,
            Y = 300,
            WindowTitle = "SCP-087-B",
        });

        Window.CursorVisible = false;

        GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, new() {
            Debug = Debug,
            SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt,
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
        }, GraphicsBackend.Direct3D11);

        if (GraphicsDevice.IsClipSpaceYInverted) {
            throw new("Graphics device does not support a correctly oriented clip space, this is currently unsupported!");
        }

        if (!GraphicsDevice.IsDepthRangeZeroToOne) {
            Log.Warning("Desired depth range is not supported by the graphics device." +
                               " Visual glitches may occur.");
        }

        ShaderCache = new(this);
        TextureCache = new(this);

        MainTarget = new(GraphicsDevice);

        GraphicsDevice.GetOpenGLInfo(out var info);
        _preferredShaderFileExtension = GraphicsDevice.BackendType switch {
            GraphicsBackend.Direct3D11 => new [] { "hlsl.dxbc", "hlsl" },
            GraphicsBackend.OpenGL => new[] { "330.glsl" }, //TODO: Reinvestigate?
                //GetOpenGLShaderVersion() >= 450
                //? "450.glsl" + (/*info.Extensions.Contains("GL_ARB_gl_spirv") ? ".spv" :*/ "")
                //: "330.glsl",
            GraphicsBackend.OpenGLES => new[] { "300.glsles" },
            GraphicsBackend.Vulkan => new [] { "450.glsl.spv" },
            GraphicsBackend.Metal => new [] { "metallib" },
        };

        int GetOpenGLShaderVersion()
            => int.Parse(info.ShadingLanguageVersion[..info.ShadingLanguageVersion.IndexOf(' ')].Replace(".", ""));

        MissingTexture = TextureCache.GetTexture("Assets/Textures/missing.png");
    }

    public RoomData LoadRoom(PhysicsResources physics, string name)
        => _roomProviderCollector.LoadRoom(this, physics, name);

    private readonly string[] _preferredShaderFileExtension;
    // TODO: We can probably support HLSL here as well.
    // Can we support other GLSL versions?
    private static readonly string[] _fallbackShaderFileExtension = { "450.glsl.spv", "450.glsl" };

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
                => File.Exists($"{shaderName}/vertex.{x.Extension}")
                && File.Exists($"{shaderName}/fragment.{x.Extension}"));

        if (t == default) {
            throw new($"Shader {shaderName} not found with suitable extension!");
        }

        needsSpirVBridge = t.RequiresSpirVBridge;
        return t.Extension;
    }

    protected override void DisposeImpl() {
        ShaderCache.Dispose();
        TextureCache.Dispose();
        MainTarget.Dispose();
        GraphicsDevice.Dispose();
    }
}
