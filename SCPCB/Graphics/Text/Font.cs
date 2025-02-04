using System.Numerics;
using FreeTypeSharp;
using FreeTypeSharp.Native;
using SCPCB.Graphics.Primitives;
using SCPCB.Utility;
using Veldrid;
using static FreeTypeSharp.Native.FT;

namespace SCPCB.Graphics.Text;

public class Font : Disposable {
    private readonly GraphicsResources _gfxRes;
    private readonly FreeTypeFaceFacade _face;
    private readonly nint _faceNative;

    public readonly record struct GlyphInfo(uint Index, ICBTexture Atlas, Vector2 UvPosition, Vector2 Dimensions, Vector2 Offset, Vector2 Advance);

    private readonly Dictionary<char, GlyphInfo> _glyphs = [];
    private GlyphInfo? _undefinedGlyph;

    private CBTexture _currAtlas;
    private uint _currRowHeight;
    private uint _currX;
    private uint _currY;

    private const uint GLYPH_PADDING = 2;
    public const uint ATLAS_SIZE = 2048;

    public Font(GraphicsResources gfxRes, FreeTypeLibrary lib, string path, int size) {
        Log.Information("Loading font {Path}", path);

        FT_New_Face(lib.Native, path, 0, out _faceNative);
        _gfxRes = gfxRes;
        _face = new(lib, _faceNative);
        _face.SelectCharSize(size, 0, 0);
        MakeNewAtlas();

        // None of the built-in metrics really fit here.
        Height = GetGlyphInfo('T').Dimensions.Y;
    }

    private void MakeNewAtlas() {
        _currAtlas = new(_gfxRes, ATLAS_SIZE, ATLAS_SIZE, PixelFormat.R8_UNorm);
    }

    private unsafe GlyphInfo GenerateGlyphInfo(char ch, uint glyphIndex) {
        FT_Load_Glyph(_faceNative, glyphIndex, 0);
        FT_Render_Glyph((nint)_face.GlyphSlot, FT_Render_Mode.FT_RENDER_MODE_NORMAL);
        var bmp = _face.GlyphBitmap;

        if (bmp.width > ATLAS_SIZE || bmp.rows > ATLAS_SIZE) {
            throw new($"Glyph for '{ch}' too large to render on atlas ({bmp.width}x{bmp.rows} vs {ATLAS_SIZE}x{ATLAS_SIZE})!");
        }

        if (_currX + bmp.width > ATLAS_SIZE) {
            _currX = 0;
            _currY += _currRowHeight + GLYPH_PADDING;
            _currRowHeight = 0;
        }

        _currRowHeight = Math.Max(_currRowHeight, bmp.rows);
        if (_currY + _currRowHeight > ATLAS_SIZE) {
            MakeNewAtlas();
            _currX = 0;
            _currY = 0;
        }

        _currAtlas.Update(new(bmp.buffer.ToPointer(), (int)(bmp.width * bmp.rows)), _currX, _currY, bmp.width, bmp.rows);

        var info = new GlyphInfo(glyphIndex, _currAtlas, new(_currX, _currY), new(bmp.width, bmp.rows),
            new(_face.GlyphBitmapLeft, _face.GlyphBitmapTop),
            new(_face.GlyphMetricHorizontalAdvance, _face.GlyphMetricVerticalAdvance));

        _currX += bmp.width + GLYPH_PADDING;

        return info;
    }

    public GlyphInfo GetGlyphInfo(char ch) {
        if (_glyphs.TryGetValue(ch, out var info)) {
            return info;
        }

        var index = _face.GetCharIndex(ch);

        var newInfo = index == 0 ? (_undefinedGlyph ??= GenerateGlyphInfo(ch, 0)) : GenerateGlyphInfo(ch, index);
        _glyphs.Add(ch, newInfo);

        return newInfo;
    }

    public float VerticalAdvance => _face.GlyphMetricVerticalAdvance;
    public float Height { get; }

    protected override void DisposeImpl() {
        FT_Done_Face(_faceNative);
        foreach (var atlas in _glyphs
                     .Select(x => x.Value.Atlas)
                     .Distinct()) {
            atlas.Dispose();
        }
    }
}
