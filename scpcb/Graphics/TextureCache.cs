using System.Drawing;
using Veldrid;

namespace scpcb.Graphics; 

public class TextureCache : Disposable {
    // TODO: Turn all of these into weak references?
    // Make sure to NOT allow for losing a reference to a texture!
    private readonly Dictionary<string, ICBTexture> _textures = new();
    private readonly Dictionary<Color, ICBTexture> _colorTextures = new();

    private readonly GraphicsDevice _gfx;

    public TextureCache(GraphicsDevice gfx) {
        _gfx = gfx;
    }

    public ICBTexture GetTexture(string filename) {
        if (_textures.TryGetValue(filename, out var texture)) {
            return texture;
        }

        var newTexture = new CBTexture(_gfx, filename);
        _textures.Add(filename, newTexture);
        return newTexture;
    }

    public ICBTexture GetTexture(Color color) {
        if (_colorTextures.TryGetValue(color, out var texture)) {
            return texture;
        }

        var newTexture = new CBTexture(_gfx, color);
        _colorTextures.Add(color, newTexture);
        return newTexture;
    }

    protected override void DisposeImpl() {
        foreach (var tex in _textures.Values.Concat(_colorTextures.Values)) {
            if (tex is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
