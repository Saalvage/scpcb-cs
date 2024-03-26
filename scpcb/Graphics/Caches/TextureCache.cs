using System.Drawing;
using scpcb.Graphics.Primitives;
using scpcb.Utility;
using Serilog;

namespace scpcb.Graphics.Caches;

public class TextureCache : Disposable {
    private readonly WeakDictionary<string, ICBTexture> _textures = [];
    private readonly WeakDictionary<Color, ICBTexture> _colorTextures = [];

    private readonly GraphicsResources _gfxRes;

    public TextureCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public ICBTexture GetTexture(string filename) {
        if (_textures.TryGetValue(filename, out var texture)) {
            return texture;
        }

        ICBTexture newTexture;
        try {
            newTexture = CBTexture.Load(_gfxRes, filename);
        } catch (Exception e) {
            newTexture = _gfxRes.MissingTexture;
            Log.Warning(e, "Could not load texture {filename}", filename);
        }
        _textures.Add(filename, newTexture);
        return newTexture;
    }

    public ICBTexture GetTexture(Color color) {
        if (_colorTextures.TryGetValue(color, out var texture)) {
            return texture;
        }

        var newTexture = CBTexture.FromColor(_gfxRes, color);
        _colorTextures.Add(color, newTexture);
        return newTexture;
    }

    protected override void DisposeImpl() {
        foreach (var texture in _textures.Select(x => x.Value)
                     .Concat(_colorTextures.Select(x => x.Value))) {
            texture.Dispose();
        }
    }
}
