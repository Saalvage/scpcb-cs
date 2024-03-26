using System.Drawing;
using scpcb.Graphics.Textures;
using scpcb.Utility;
using Serilog;
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
    // TODO: Find alternative design that allows for immutability?
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

    public CBTexture(GraphicsResources gfxRes, uint width, uint height, Span<byte> data, PixelFormat format, bool useMips = true) {
        Log.Information("Loading {width}x{height} texture from {bytes} bytes", width, height, data.Length);

        Width = width;
        Height = height;
        var gfx = gfxRes.GraphicsDevice;
        _texture = gfx.ResourceFactory.CreateTexture(new(Width, Height, 1, useMips ? 4u : 1u, 1, format,
            TextureUsage.Sampled | TextureUsage.GenerateMipmaps, TextureType.Texture2D));
        gfx.UpdateTexture(_texture, data, 0, 0, 0, Width, Height, 1, 0, 0);
        View = gfx.ResourceFactory.CreateTextureView(_texture);

        if (useMips) {
            gfxRes.RegisterForMipmapGeneration(this);
        }
    }

    public void GenerateMipmaps(CommandList commands) {
        commands.GenerateMipmaps(_texture);
    }

    protected override void DisposeImpl() {
        View?.Dispose();
        _texture?.Dispose();
    }
}
