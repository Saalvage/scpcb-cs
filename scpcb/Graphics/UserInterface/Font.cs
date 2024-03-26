using FreeTypeSharp;
using scpcb.Graphics.Primitives;
using scpcb.Utility;
using Veldrid;

namespace scpcb.Graphics.UserInterface;

using FreeTypeSharp.Native;
using static FreeTypeSharp.Native.FT;

public class Font : Disposable {
    private readonly GraphicsResources _gfxRes;
    private readonly FreeTypeFaceFacade _face;
    private readonly nint _faceNative;

    public Font(GraphicsResources gfxRes, FreeTypeLibrary lib, string path) {
        FT_New_Face(lib.Native, path, 0, out _faceNative);
        _gfxRes = gfxRes;
        _face = new(lib, _faceNative);
        _face.SelectCharSize(16 * 64, 0, 0);
    }

    public unsafe ICBTexture LoadGlyph(char ch) {
        var index = _face.GetCharIndex(ch);
        FT_Load_Glyph(_faceNative, index, 0);
        FT_Render_Glyph((nint)_face.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_NORMAL);
        var bmp = _face.GlyphBitmap;
        return new CBTexture(_gfxRes, bmp.width, bmp.rows, new(bmp.buffer.ToPointer(), (int)(bmp.width * bmp.rows)), PixelFormat.R8_UNorm);
    }

    protected override void DisposeImpl() {
        FT_Done_Face(_faceNative);
    }
}
