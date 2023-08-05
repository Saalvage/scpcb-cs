using System.Drawing;
using scpcb.Graphics.Primitives;
using scpcb.Utility;

namespace scpcb.Graphics.Utility;

public class TextureCache : Disposable {
    // TODO: Turn all of these into weak references?
    // Make sure to NOT allow for losing a reference to a texture!
    private readonly Dictionary<string, WeakReference<ICBTexture>> _textures = new();
    private readonly Dictionary<Color, WeakReference<ICBTexture>> _colorTextures = new();

    private readonly GraphicsResources _gfxRes;

    public TextureCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public ICBTexture GetTexture(string filename) {
        if (_textures.TryGetValue(filename, out var textureWeak) && textureWeak.TryGetTarget(out var texture)) {
            return texture;
        }

        var newTexture = new CBTexture(_gfxRes, filename);
        _textures[filename] = new(newTexture);
        return newTexture;
    }

    public ICBTexture GetTexture(Color color) {
        if (_colorTextures.TryGetValue(color, out var textureWeak) && textureWeak.TryGetTarget(out var texture)) {
            return texture;
        }

        var newTexture = new CBTexture(_gfxRes, color);
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
