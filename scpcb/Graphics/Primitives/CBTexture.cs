using System.Drawing;
using scpcb.Graphics.Textures;
using scpcb.Utility;
using StbImageSharp;
using Veldrid;

namespace scpcb.Graphics.Primitives;

public interface ICBTexture : IDisposable {
    TextureView View { get; }
    uint Width { get; }
    uint Height { get; }

    /// <summary>
    /// Whether the underlying view might change.
    /// </summary>
    /// <remarks>
    /// The texture itself can change without issue (e.g. video playback),
    /// this should only be true if the underlying view can be switched out.
    /// </remarks>
    bool IsStatic => true;
}

public class CBTexture : Disposable, IMipmappable {
    private readonly GraphicsDevice _gfx;

    private readonly Texture _texture;
    public TextureView View { get; }
    public uint Width { get; }
    public uint Height { get; }

    public static CBTexture Load(GraphicsResources gfxRes, string path) {
        Log.Information("Loading texture {file}", path);

        using var stream = File.OpenRead(path);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        return new(gfxRes, (uint)image.Width, (uint)image.Height, image.Data, PixelFormat.R8_G8_B8_A8_UNorm, true);
    }

    public static CBTexture FromColor(GraphicsResources gfxRes, Color color) {
        Log.Information("Creating texture with {color}", color);

        return new(gfxRes, 1, 1, [color.R, color.G, color.B, color.A], PixelFormat.R8_G8_B8_A8_UNorm, false);
    }

    private CBTexture(GraphicsResources gfxRes, uint width, uint height, PixelFormat format, TextureUsage usage) {
        _gfx = gfxRes.GraphicsDevice;

        Width = width;
        Height = height;

        _texture = _gfx.ResourceFactory.CreateTexture(new(Width, Height, 1, 1, 1, format,
            usage, TextureType.Texture2D));
        View = _gfx.ResourceFactory.CreateTextureView(_texture);
    }

    public CBTexture(GraphicsResources gfxRes, uint width, uint height, PixelFormat format)
            : this(gfxRes, width, height, format, TextureUsage.Sampled) {
        Log.Information("Creating {width}x{height} blank texture", width, height);
    }

    public CBTexture(GraphicsResources gfxRes, uint width, uint height, Span<byte> data, PixelFormat format, bool useMips = true)
            : this(gfxRes, width, height, format, TextureUsage.Sampled | (useMips ? TextureUsage.GenerateMipmaps : 0)) {
        Log.Information("Loading {width}x{height} texture", width, height);

        _gfx.UpdateTexture(_texture, data, 0, 0, 0, Width, Height, 1, 0, 0);

        if (useMips) {
            gfxRes.RegisterForMipmapGeneration(this);
        }
    }

    public void GenerateMipmaps(CommandList commands) {
        commands.GenerateMipmaps(_texture);
    }

    // TODO: I'm not sure if this is a good design, we're effectively bundling a texture with its corresponding view
    // and adding a bunch of utility methods while removing some complexity.
    // Especially if we're going to have multiple implementations of ICBTexture (like the video texture),
    // maybe an abstract base impl with a few abstractions would be preferable to the current design.
    public void Update(Span<byte> bytes, uint x, uint y, uint width, uint height) {
        _gfx.UpdateTexture(_texture, bytes, x, y, 0, width, height, 1, 0, 0);
    }

    protected override void DisposeImpl() {
        View?.Dispose();
        _texture?.Dispose();
    }
}
