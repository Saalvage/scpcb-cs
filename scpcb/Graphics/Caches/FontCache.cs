using FreeTypeSharp;
using scpcb.Graphics.UserInterface.Utility;

namespace scpcb.Graphics.Caches;

public class FontCache : BaseCache<(string, int), Font> {
    private readonly GraphicsResources _gfxRes;
    private readonly FreeTypeLibrary _freeType;

    public FontCache(GraphicsResources gfxRes, FreeTypeLibrary lib) {
        _gfxRes = gfxRes;
        _freeType = lib;
    }

    public Font GetFont(string filename, int size, bool useRawSize = false) {
        size = useRawSize ? size : (size * Math.Min(_gfxRes.Window.Height, _gfxRes.Window.Width) / 1024);
        return _dic.TryGetValue((filename, size), out var font)
            ? font
            : _dic[(filename, size)] = new(_gfxRes, _freeType, filename, size);
    }
}
