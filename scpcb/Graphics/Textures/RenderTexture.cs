using scpcb.Graphics.Primitives;
using Veldrid;

namespace scpcb.Graphics.Textures;

// TODO: Of course, OpenGL doesn't work with recursive render textures because of its Y-flip.
public class RenderTexture : RenderTarget, ICBTexture {
    private record RenderSet(Texture Texture, TextureView View, Framebuffer Buffer) : IDisposable {
        public static RenderSet Create(ResourceFactory factory, Texture depth, uint width, uint height) {
            var tex = factory.CreateTexture(new(width, height, 1, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled, TextureType.Texture2D));
            var view = factory.CreateTextureView(tex);
            var buffer = factory.CreateFramebuffer(new(depth, tex));
            return new(tex, view, buffer);
        }

        public void Dispose() {
            View.Dispose();
            Buffer.Dispose();
            Texture.Dispose();
        }
    }

    private readonly RenderSet _setA;
    private readonly RenderSet? _setB;

    private bool _renderingToA = true;

    private readonly Texture _depth;

    public TextureView View => (!_recursive ? _setA : _renderingToA ? _setB : _setA)!.View;

    protected override Framebuffer Framebuffer => (_renderingToA ? _setA : _setB)!.Buffer;

    public uint Width { get; }
    
    public uint Height { get; }

    private readonly bool _recursive;

    public RenderTexture(GraphicsResources gfxRes, uint width, uint height, bool recursive = false) : base(gfxRes.GraphicsDevice) {
        _recursive = recursive;

        Width = width;
        Height = height;

        var factory = gfxRes.GraphicsDevice.ResourceFactory;

        _depth = factory.CreateTexture(new(width, height, 1, 1, 1, PixelFormat.D24_UNorm_S8_UInt,
            TextureUsage.DepthStencil, TextureType.Texture2D)); 

        _setA = RenderSet.Create(factory, _depth, width, height);
        if (_recursive) {
            _setB = RenderSet.Create(factory, _depth, width, height);
        }
    }

    public override void Start() {
        if (_recursive) {
            _renderingToA = !_renderingToA;
        }
        base.Start();
    }

    public bool IsStatic => !_recursive;

    protected override void DisposeImpl() {
        _setA.Dispose();
        _setB?.Dispose();
        _depth.Dispose();
        base.DisposeImpl();
    }
}
