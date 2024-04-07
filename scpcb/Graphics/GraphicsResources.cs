using FreeTypeSharp;
using scpcb.Graphics.Caches;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Textures;
using scpcb.Graphics.UserInterface;
using scpcb.Map;
using scpcb.Map.RoomProviders;
using scpcb.Physics;
using scpcb.Scenes;
using scpcb.Utility;
using Serilog;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace scpcb.Graphics;

public class GraphicsResources : Disposable {
    private class MainRenderTarget : RenderTarget {
        private readonly GraphicsResources _gfxRes;

        public MainRenderTarget(GraphicsResources gfxRes, Framebuffer buffer) : base(gfxRes.GraphicsDevice) {
            _gfxRes = gfxRes;
            Framebuffer = buffer;
        }

        public override void Start() {
            base.Start();
            foreach (var t in _gfxRes._generateMipTextures) {
                t.GenerateMipmaps(_commands);
            }
            _gfxRes._generateMipTextures.Clear();
        }

        public override void End() {
            base.End();
            _gfx.SwapBuffers();
        }
    }

    public GraphicsDevice GraphicsDevice { get; }

    public ShaderCache ShaderCache { get; }
    public TextureCache TextureCache { get; }
    public MaterialCache MaterialCache { get; }
    public MeshCache MeshCache { get; }
    public FontCache FontCache { get; }

    public ICBTexture MissingTexture { get; }

    public Sdl2Window Window { get; }

    private readonly MainRenderTarget _mainTarget;
    public RenderTarget MainTarget => _mainTarget;

    public bool Debug { get; }

    public Sampler ClampAnisoSampler { get; }
    public Sampler WrapAnisoSampler { get; }

    private readonly RoomProviderCollector _roomProviderCollector = new();

    private readonly List<IMipmappable> _generateMipTextures = [];

    private readonly FreeTypeLibrary _freeType = new();

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

        var backend = GraphicsBackend.Direct3D11;
        Log.Information("Starting game {backend} ({width}x{height}) DEBUG={debug}", backend, width, height, debug);

        GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, new() {
            Debug = Debug,
            SwapchainDepthFormat = PixelFormat.D24_UNorm_S8_UInt,
            PreferStandardClipSpaceYDirection = true,
            PreferDepthRangeZeroToOne = true,
        }, backend);

        if (GraphicsDevice.IsClipSpaceYInverted) {
            throw new("Graphics device does not support a correctly oriented clip space, this is currently unsupported!");
        }

        if (!GraphicsDevice.IsDepthRangeZeroToOne) {
            Log.Warning("Desired depth range is not supported by the graphics device." +
                               " Visual glitches may occur.");
        }

        ShaderCache = new(this);
        TextureCache = new(this);
        MaterialCache = new(GraphicsDevice, ShaderCache);
        MeshCache = new(this);
        FontCache = new(this, _freeType);

        _mainTarget = new(this, GraphicsDevice.SwapchainFramebuffer);

        var baseSamplerDesc = new SamplerDescription() {
            Filter = SamplerFilter.Anisotropic,
            LodBias = 0,
            MinimumLod = 0,
            MaximumLod = uint.MaxValue,
            MaximumAnisotropy = 4,
        };

        ClampAnisoSampler = GraphicsDevice.ResourceFactory.CreateSampler(baseSamplerDesc with {
            AddressModeU = SamplerAddressMode.Clamp,
            AddressModeV = SamplerAddressMode.Clamp,
            AddressModeW = SamplerAddressMode.Clamp,
        });

        WrapAnisoSampler = GraphicsDevice.ResourceFactory.CreateSampler(baseSamplerDesc with {
            AddressModeU = SamplerAddressMode.Wrap,
            AddressModeV = SamplerAddressMode.Wrap,
            AddressModeW = SamplerAddressMode.Wrap,
        });

        GraphicsDevice.GetOpenGLInfo(out var info);
        _preferredShaderFileExtension = GraphicsDevice.BackendType switch {
            GraphicsBackend.Direct3D11 => ["hlsl.dxbc", "hlsl"],
            GraphicsBackend.OpenGL => ["330.glsl"], //TODO: Reinvestigate?
                //GetOpenGLShaderVersion() >= 450
                //? "450.glsl" + (/*info.Extensions.Contains("GL_ARB_gl_spirv") ? ".spv" :*/ "")
                //: "330.glsl",
            GraphicsBackend.OpenGLES => ["300.glsles"],
            GraphicsBackend.Vulkan => ["450.glsl.spv"],
            GraphicsBackend.Metal => ["metallib"],
        };

        int GetOpenGLShaderVersion()
            => int.Parse(info.ShadingLanguageVersion[..info.ShadingLanguageVersion.IndexOf(' ')].Replace(".", ""));

        MissingTexture = TextureCache.GetTexture("Assets/Textures/missing.png");
    }

    public void RegisterForMipmapGeneration(IMipmappable texture) {
        _generateMipTextures.Add(texture);
    }

    public IRoomData LoadRoom(IScene scene, PhysicsResources physics, string name)
        => _roomProviderCollector.LoadRoom(scene, this, physics, name);

    private readonly string[] _preferredShaderFileExtension;
    // TODO: We can probably support HLSL here as well.
    // Can we support other GLSL versions?
    private static readonly string[] _fallbackShaderFileExtension = ["450.glsl.spv", "450.glsl"];

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
        _freeType.Dispose();
        ShaderCache.Dispose();
        TextureCache.Dispose();
        MaterialCache.Dispose();
        MeshCache.Dispose();
        MainTarget.Dispose();
        ClampAnisoSampler.Dispose();
        GraphicsDevice.Dispose();
    }
}
