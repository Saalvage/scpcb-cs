using System.Numerics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Textures;
using scpcb.Graphics.UserInterface.Utility;

namespace scpcb.Graphics.UserInterface.Primitives;

using AtlasMesh = (ICBTexture, ICBMesh<TextShader.Vertex>);

public class TextElement : UIElement {
    private readonly ICBShader _shader;
    private readonly Font _font;
    private readonly GraphicsResources _gfxRes;

    private AtlasMesh[] _meshes;
    private bool _dirtyMesh = true;

    private record Chunk(int Begin, int Count) {
        public int CurrentIndex = Begin;
        public int End => Begin + Count;
    }

    private Vector2[] _offsets;
    public IReadOnlyList<Vector2> Offsets {
        get {
            GenerateMeshes();
            return _offsets;
        }
    }

    private void GenerateMeshes() {
        if (!_dirtyMesh) {
            return;
        }

        // TODO: Allocate on heap when too big.
        Span<TextShader.Vertex> vertices = stackalloc TextShader.Vertex[Text.Length * 4];
        Span<uint> indices = stackalloc uint[Text.Length * 6];

        var runningTotal = 0;
        var dataChunks = Text
            .GroupBy(x => _font.GetGlyphInfo(x).Atlas)
            .ToDictionary(x => x.Key, x => {
                var ret = new Chunk(runningTotal, x.Count());
                runningTotal += ret.Count;
                return ret;
            });

        var ret = new(Font.GlyphInfo Info, float Offset)[Text.Length];
        var offset = new Vector2();
        _offsets = new Vector2[Text.Length + 1];
        var width = 0f;
        char? prevLineEnding = null;
        for (var i = 0; i < Text.Length; i++) {
            _offsets[i] = offset;

            var glyphInfo = _font.GetGlyphInfo(Text[i]);
            ret[i] = new(glyphInfo, 0f);

            if (Text[i] is '\n' or '\r') {
                if (prevLineEnding == null || prevLineEnding == Text[i]) {
                    prevLineEnding = Text[i];
                    width = MathF.Max(width, offset.X);
                    offset.X = 0;
                    offset.Y -= _font.VerticalAdvance;
                }
                continue;
            } else {
                prevLineEnding = null;
            }

            var baseOffset = glyphInfo.Offset + offset - new Vector2(0f, glyphInfo.Dimensions.Y);
            baseOffset *= Scale;
            var scaledGlyphDimensions = Scale * glyphInfo.Dimensions;
            var chunk = dataChunks[glyphInfo.Atlas];
            var spanIndex = chunk.CurrentIndex++;
            vertices[spanIndex * 4 + 0] = new(baseOffset, (glyphInfo.UvPosition + new Vector2(0, glyphInfo.Dimensions.Y)) / Font.ATLAS_SIZE);
            vertices[spanIndex * 4 + 1] = new(baseOffset + new Vector2(scaledGlyphDimensions.X, 0), (glyphInfo.UvPosition + glyphInfo.Dimensions) / Font.ATLAS_SIZE);
            vertices[spanIndex * 4 + 2] = new(baseOffset + new Vector2(0, scaledGlyphDimensions.Y), glyphInfo.UvPosition / Font.ATLAS_SIZE);
            vertices[spanIndex * 4 + 3] = new(baseOffset + scaledGlyphDimensions, (glyphInfo.UvPosition + new Vector2(glyphInfo.Dimensions.X, 0)) / Font.ATLAS_SIZE);

            var localIndex = spanIndex - chunk.Begin;
            indices[spanIndex * 6 + 0] = (uint)localIndex * 4 + 0;
            indices[spanIndex * 6 + 1] = (uint)localIndex * 4 + 1;
            indices[spanIndex * 6 + 2] = (uint)localIndex * 4 + 2;
            indices[spanIndex * 6 + 3] = (uint)localIndex * 4 + 3;
            indices[spanIndex * 6 + 4] = (uint)localIndex * 4 + 2;
            indices[spanIndex * 6 + 5] = (uint)localIndex * 4 + 1;
            offset.X += glyphInfo.Advance.X;
        }
        width = MathF.Max(width, offset.X);
        _offsets[Text.Length] = offset;

        // TODO: Reusing meshes or buffers (maybe from a pool) might make sense here.
        _meshes = new AtlasMesh[dataChunks.Count];
        foreach (var ((key, x), i) in dataChunks.Zip(Enumerable.Range(0, _meshes.Length))) {
            _meshes[i] = (key, new CBMesh<TextShader.Vertex>(_gfxRes.GraphicsDevice,
                vertices[(4 * x.Begin)..(4 * x.End)],
                indices[(6 * x.Begin)..(6 * x.End)]));
        }

        _dimensionsInternal = new(width, -offset.Y + _font.Height);

        _dirtyMesh = false;
    }

    private Vector2 _dimensionsInternal;
    private Vector2 _dimensions {
        get {
            GenerateMeshes();
            return _dimensionsInternal;
        }
    }

    private string _text = "";
    public string Text {
        get => _text;
        set {
            _dirtyMesh |= _text != value;
            _text = value;
        }
    }

    public Vector2 Scale { get; set; } = Vector2.One;

    public override Vector2 PixelSize {
        get => _dimensions * Scale;
        set => Scale = value / _dimensions;
    }

    public TextElement(GraphicsResources gfxRes, Font font) {
        _gfxRes = gfxRes;
        _font = font;
        _shader = gfxRes.ShaderCache.GetShader<TextShader>();
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position) {
        GenerateMeshes();

        var constants = _shader.Constants!;

        var halfDimension = _dimensions * 0.5f;
        halfDimension.Y *= -1;

        constants.SetValue<IPositionConstantMember, Vector3>(new(position + Scale * (new Vector2(0f, -_font.Height) - halfDimension), Z));
        foreach (var (tex, mesh) in _meshes) {
            var mat = _gfxRes.MaterialCache.GetMaterial<TextShader, TextShader.Vertex>([tex], [_gfxRes.ClampAnisoSampler]);
            target.Render<TextShader.Vertex>(new(mesh, mat));
        }
    }
}
