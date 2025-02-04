using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Utility;

namespace SCPCB.Graphics.Text;

public class TextModel : Disposable {
    private record Chunk(int Begin, int Count) {
        public int CurrentIndex = Begin;
        public int End => Begin + Count;
    }

    private readonly GraphicsResources _gfxRes;
    private readonly ICBShader<VPositionTexture2D> _shader;
    private readonly Font _font;

    public Vector2 Dimensions { get; private set; }

    private bool _dirty = true;

    public string Text {
        get;
        set {
            _dirty |= field != value;
            field = value;
        }
    } = "";

    private readonly IConstantHolder? _constants;

    public IReadOnlyList<Vector2> Offsets {
        get {
            GenerateMeshes();
            return field;
        }
        private set;
    }

    public IReadOnlyList<IMeshInstance<VPositionTexture2D>> Meshes {
        get {
            GenerateMeshes();
            return field;
        }
        private set;
    }

    public TextModel(GraphicsResources gfxRes, Font font, ICBShader<VPositionTexture2D> shader) {
        _gfxRes = gfxRes;
        _shader = shader;
        _font = font;
        _constants = shader.TryCreateInstanceConstants();
    }

    private void GenerateMeshes() {
        if (!_dirty) {
            return;
        }

        // TODO: Allocate on heap when too big.
        Span<VPositionTexture2D> vertices = stackalloc VPositionTexture2D[Text.Length * 4];
        Span<uint> indices = stackalloc uint[Text.Length * 6];

        var runningTotal = 0;
        var dataChunks = Text
            .GroupBy(x => _font.GetGlyphInfo(x).Atlas)
            .ToDictionary(x => x.Key, x => {
                var ret = new Chunk(runningTotal, x.Count());
                runningTotal += ret.Count;
                return ret;
            });

        var ret = new (Font.GlyphInfo Info, float Offset)[Text.Length];
        var offset = Vector2.Zero;
        var offsets = new Vector2[Text.Length + 1];
        var width = 0f;
        char? prevLineEnding = null;
        for (var i = 0; i < Text.Length; i++) {
            offsets[i] = offset;

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

            var baseOffset = glyphInfo.Offset + offset - new Vector2(0, _font.Height);
            var chunk = dataChunks[glyphInfo.Atlas];
            var spanIndex = chunk.CurrentIndex++;
            vertices[spanIndex * 4 + 0] = new(baseOffset, glyphInfo.UvPosition / Font.ATLAS_SIZE);
            vertices[spanIndex * 4 + 1] = new(baseOffset - new Vector2(0, glyphInfo.Dimensions.Y),
                (glyphInfo.UvPosition + new Vector2(0, glyphInfo.Dimensions.Y)) / Font.ATLAS_SIZE);
            vertices[spanIndex * 4 + 2] = new(baseOffset + new Vector2(glyphInfo.Dimensions.X, 0),
                (glyphInfo.UvPosition + new Vector2(glyphInfo.Dimensions.X, 0)) / Font.ATLAS_SIZE);
            vertices[spanIndex * 4 + 3] = new(baseOffset + new Vector2(glyphInfo.Dimensions.X, -glyphInfo.Dimensions.Y),
                (glyphInfo.UvPosition + glyphInfo.Dimensions) / Font.ATLAS_SIZE);

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
        offsets[Text.Length] = offset;

        // TODO: Reusing meshes or buffers (maybe from a pool) would make sense here.
        var meshes = new MeshInstance<VPositionTexture2D>[dataChunks.Count];
        foreach (var ((key, x), i) in dataChunks.Zip(Enumerable.Range(0, meshes.Length))) {
            meshes[i] = new(_constants,
                _gfxRes.MaterialCache.GetMaterial(_shader, [key], [_gfxRes.ClampAnisoSampler]),
                new CBMesh<VPositionTexture2D>(_gfxRes.GraphicsDevice,
                    vertices[(4 * x.Begin)..(4 * x.End)],
                    indices[(6 * x.Begin)..(6 * x.End)]));
        }

        Offsets = offsets;
        Meshes = meshes;
        Dimensions = new(width, -offset.Y + _font.Height);

        _dirty = false;
    }

    protected override void DisposeImpl() {
        foreach (var i in Meshes) {
            i.Mesh.Dispose();
        }
        _constants?.Dispose();
    }
}
