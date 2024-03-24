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
    private readonly Texture _texture;
    public TextureView View { get; }
    public uint Width { get; }
    public uint Height { get; }

    public CBTexture(GraphicsResources gfxRes, Color color) {
        Log.Information("Creating texture with {color}", color);

        Width = 1;
        Height = 1;
        var gfx = gfxRes.GraphicsDevice;
        _texture = gfx.ResourceFactory.CreateTexture(new(1, 1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
        Span<byte> bytes = stackalloc byte[4];
        bytes[0] = color.R;
        bytes[1] = color.G;
        bytes[2] = color.B;
        bytes[3] = color.A;
        gfx.UpdateTexture(_texture, bytes, 0, 0, 0, 1, 1, 1, 0, 0);
        View = gfx.ResourceFactory.CreateTextureView(_texture);
    }

    public CBTexture(GraphicsResources gfxRes, string file) {
        Log.Information("Loading texture {file}", file);

        using var stream = File.OpenRead(file);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        Width = (uint)image.Width;
        Height = (uint)image.Height;
        // TODO: OPT Use unmanaged byte array?
        var gfx = gfxRes.GraphicsDevice;
        _texture = gfx.ResourceFactory.CreateTexture(new(Width, Height, 1, 4, 1, PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled | TextureUsage.GenerateMipmaps, TextureType.Texture2D));
        gfx.UpdateTexture(_texture, image.Data, 0, 0, 0, Width, Height, 1, 0, 0);
        View = gfx.ResourceFactory.CreateTextureView(_texture);

        gfxRes.RegisterForMipmapGeneration(this);
    }

    public void GenerateMipmaps(CommandList commands) {
        commands.GenerateMipmaps(_texture);
    }

    protected override void DisposeImpl() {
        // TODO: We only need to use null-coalescing operators here
        // because we have to deal with partially constructed objects..
        View?.Dispose();
        _texture?.Dispose();
    }
}
