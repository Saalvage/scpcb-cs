using System.Drawing;
using StbImageSharp;
using Veldrid;

namespace scpcb;

public interface ICBTexture {
    TextureView View { get; }
    uint Width { get; }
    uint Height { get; }
}

public class CBTexture : Disposable, ICBTexture {
    private readonly Texture _texture;
    public TextureView View { get; }
    public uint Width { get; }
    public uint Height { get; }

    public CBTexture(GraphicsDevice gfx, Color color) {
        Width = 1;
        Height = 1;
        _texture = gfx.ResourceFactory.CreateTexture(new(1, 1, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
        Span<byte> bytes = stackalloc byte[4];
        bytes[0] = color.R;
        bytes[1] = color.G;
        bytes[2] = color.B;
        bytes[3] = color.A;
        gfx.UpdateTexture(_texture, bytes, 0, 0, 0, 1, 1, 1, 0, 0);
        View = gfx.ResourceFactory.CreateTextureView(_texture);
    }

    public CBTexture(GraphicsDevice gfx, string file) {
        using var stream = File.OpenRead(file);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        Width = (uint)image.Width;
        Height = (uint)image.Height;
        // TODO: OPT Use unmanaged byte array?
        _texture = gfx.ResourceFactory.CreateTexture(new(Width, Height, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
        gfx.UpdateTexture(_texture, image.Data, 0, 0, 0, Width, Height, 1, 0, 0);
        View = gfx.ResourceFactory.CreateTextureView(_texture);
    }

    protected override void DisposeImpl() {
        View.Dispose();
        _texture.Dispose();
    }
}
