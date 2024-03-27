using System.Numerics;
using scpcb.Graphics.Caches;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Textures;

namespace scpcb.Graphics.UserInterface;

public class Letter : UIElement, ISharedMeshProvider<Letter, TextShader.Vertex> {
    private readonly ICBShader _shader;
    private readonly ICBModel _model;
    private readonly Font.GlyphInfo _glyph;

    public Letter(GraphicsResources gfxRes, Font font, char ch) {
        _glyph = font.GetGlyphInfo(ch);
        _shader = gfxRes.ShaderCache.GetShader<TextShader>();
        PixelSize = _glyph.Dimensions;
        _model = new CBModel<TextShader.Vertex>(null,
            gfxRes.MaterialCache.GetMaterial<TextShader, TextShader.Vertex>([_glyph.Atlas], [gfxRes.ClampAnisoSampler]),
            gfxRes.MeshCache.GetMesh<Letter, TextShader.Vertex>());
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position) {
        _shader.Constants!.SetValue<ITexCoordsConstantMember, Vector4>(new Vector4(_glyph.Offset.X, _glyph.Offset.X + _glyph.Dimensions.X,
            _glyph.Offset.Y, _glyph.Offset.Y + _glyph.Dimensions.Y) / Font.ATLAS_SIZE);
        _shader.Constants!.SetValue<IPositionConstantMember, Vector3>(new(position, Z));
        _shader.Constants!.SetValue<IUIScaleConstantMember, Vector2>(PixelSize);
        target.Render(_model, 0f);
    }

    public static ICBMesh<TextShader.Vertex> CreateSharedMesh(GraphicsResources gfxRes)
        => new CBMesh<TextShader.Vertex>(gfxRes.GraphicsDevice, [
            new(new(-0.5f, -0.5f)),
            new(new(0.5f, -0.5f)),
            new(new(-0.5f, 0.5f)),
            new(new(0.5f, 0.5f)),
        ], [0, 1, 2, 3, 2, 1]);
}
