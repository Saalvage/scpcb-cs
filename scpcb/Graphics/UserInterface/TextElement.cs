using System.Numerics;
using scpcb.Graphics.Caches;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Textures;

namespace scpcb.Graphics.UserInterface;

using AtlasGlyphBundle = (ICBTexture, (Font.GlyphInfo, float[])[]);

public class TextElement : UIElement, ISharedMeshProvider<TextElement, TextShader.Vertex> {
    private readonly ICBShader _shader;
    private readonly Font _font;
    private readonly ICBMesh<TextShader.Vertex> _mesh;
    private readonly GraphicsResources _gfxRes;

    private bool _generatedGlyphList = false;

    private AtlasGlyphBundle[] _glyphsInternal;
    private AtlasGlyphBundle[] _glyphs {
        get {
            GenerateGlyphList();
            return _glyphsInternal;
        }
    }

    private void GenerateGlyphList() {
        if (_generatedGlyphList) {
            return;
        }

        var ret = new(Font.GlyphInfo Info, float Offset)[Text.Length];
        var offset = 0f;
        var maxY = 0f;
        for (var i = 0; i < Text.Length; i++) {
            var glyphInfo = _font.GetGlyphInfo(Text[i]);
            ret[i] = new(glyphInfo, offset);
            offset += glyphInfo.Advance.X;
            maxY = MathF.Max(maxY, glyphInfo.Dimensions.Y);
        }

        _dimensionsInternal = new(offset, maxY);

        _glyphsInternal = ret.GroupBy(x => x.Info.Atlas)
            .Select(x => (x.Key, x 
                .GroupBy(x => x.Info.Index)
                .Select(x => (x.First().Info, x.Select(x => x.Offset).ToArray()))
                .ToArray()))
            .ToArray();

        _generatedGlyphList = true;
    }

    private Vector2 _dimensionsInternal;
    private Vector2 _dimensions {
        get {
            GenerateGlyphList();
            return _dimensionsInternal * Scale;
        }
    }

    private string _text = "";
    public string Text {
        get => _text;
        set {
            _text = value;
            _generatedGlyphList = false;
        }
    }

    // TODO: Scales are not currently working correctly on the y-axis.
    public Vector2 Scale { get; set; } = Vector2.One;

    public override Vector2 PixelSize {
        get => _dimensions * Scale;
        set => Scale = value / _dimensions;
    }

    public TextElement(GraphicsResources gfxRes, Font font) {
        _gfxRes = gfxRes;
        _font = font;
        _shader = gfxRes.ShaderCache.GetShader<TextShader>();
        _mesh = gfxRes.MeshCache.GetMesh<TextElement, TextShader.Vertex>();
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position) {
        // The math for this is fucked, trial and error away!
        var constants = _shader.Constants!;

        var halfDimension = _dimensions * 0.5f;

        foreach (var (tex, glyphs) in _glyphs) {
            var mat = _gfxRes.MaterialCache.GetMaterial<TextShader, TextShader.Vertex>([tex], [_gfxRes.ClampAnisoSampler]);
            foreach (var (glyph, instances) in glyphs) {
                constants.SetValue<ITexCoordsConstantMember, Vector4>(new Vector4(glyph.UvPosition.X, glyph.UvPosition.X + glyph.Dimensions.X,
                    glyph.UvPosition.Y, glyph.UvPosition.Y + glyph.Dimensions.Y) / Font.ATLAS_SIZE);
                constants.SetValue<IUIScaleConstantMember, Vector2>(Scale * glyph.Dimensions);

                foreach (var instOffset in instances) {
                    constants.SetValue<IPositionConstantMember, Vector3>(new(
                        position + Scale * (glyph.Offset - halfDimension + new Vector2(instOffset, 0f)), Z));
                    target.Render<TextShader.Vertex>(new(_mesh, mat));
                }
            }
        }
    }

    public static ICBMesh<TextShader.Vertex> CreateSharedMesh(GraphicsResources gfxRes)
        => new CBMesh<TextShader.Vertex>(gfxRes.GraphicsDevice, [
            new(new(0f, 0f)),
            new(new(1f, 0f)),
            new(new(0f, -1f)),
            new(new(1f, -1f)),
        ], [2, 1, 0, 1, 2, 3]);
}
