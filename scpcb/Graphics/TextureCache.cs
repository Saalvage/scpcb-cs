using System.Drawing;
using Veldrid;

namespace scpcb.Graphics; 

public class TextureCache : Disposable {
    // TODO: Turn all of these into weak references?
    // Make sure to NOT allow for losing a reference to a texture!
    private readonly Dictionary<string, WeakReference<ICBTexture>> _textures = new();
    private readonly Dictionary<Color, WeakReference<ICBTexture>> _colorTextures = new();

    private readonly GraphicsDevice _gfx;

    public TextureCache(GraphicsDevice gfx) {
        _gfx = gfx;
    }

    public ICBTexture GetTexture(string filename) {
        if (_textures.TryGetValue(filename, out var textureWeak) && textureWeak.TryGetTarget(out var texture)) {
            return texture;
        }

        var newTexture = new CBTexture(_gfx, filename);
        _textures[filename] = new(newTexture);
        return newTexture;
    }

    public ICBTexture GetTexture(Color color) {
        if (_colorTextures.TryGetValue(color, out var textureWeak) && textureWeak.TryGetTarget(out var texture)) {
            return texture;
        }

        var newTexture = new CBTexture(_gfx, color);
        _colorTextures[color] = new(newTexture);
        return newTexture;
    }

    protected override void DisposeImpl() {
        foreach (var tex in _textures.Values.Concat(_colorTextures.Values)) {
            if (tex.TryGetTarget(out var texture)) {
                texture.Dispose();
            }
        }
    }
}
