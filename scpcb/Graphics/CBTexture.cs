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

    public CBTexture(GraphicsDevice gfx) {
        Width = 100;
        Height = 100;
        _texture = gfx.ResourceFactory.CreateTexture(new(100, 100, 1, 1, 1, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled, TextureType.Texture2D));
        var bytes = new byte[100 * 100 * 4];
        Array.Fill<byte>(bytes, 255);
        for (var i = 0; i < bytes.Length; i ++) {
            bytes[i] = (byte)((double)i / bytes.Length * 256);
        }
        gfx.UpdateTexture(_texture, bytes, 0, 0, 0, 100, 100, 1, 0, 0);
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
