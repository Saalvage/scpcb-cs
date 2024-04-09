using System.Drawing;
using System.Numerics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Textures;
using scpcb.Graphics.Caches;

namespace scpcb.Graphics.UserInterface;

public class TextureElement : UIElement, ISharedMeshProvider<TextureElement, UIShader.Vertex>, IColorizableElement {
    private readonly CBModel<UIShader.Vertex> _model;

    public Color Color { get; set; } = Color.White;
    public Vector2 UvOffset { get; set; } = Vector2.Zero;
    public Vector2 UvSize { get; set; } = Vector2.One;

    public TextureElement(GraphicsResources gfxRes, ICBTexture texture, bool tile = false) {
        _model = new(null,
            gfxRes.MaterialCache.GetMaterial<UIShader, UIShader.Vertex>([texture], [tile ? gfxRes.WrapAnisoSampler : gfxRes.ClampAnisoSampler]),
            gfxRes.MeshCache.GetMesh<TextureElement, UIShader.Vertex>());
        PixelSize = new(texture.Width, texture.Height);
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position) {
        _model.Material.Shader.Constants!.SetValue<IPositionConstantMember, Vector3>(new(position, Z));
        _model.Material.Shader.Constants!.SetValue<IUIScaleConstantMember, Vector2>(PixelSize);
        _model.Material.Shader.Constants!.SetValue<IColorAlphaConstantMember, Vector4>(new Vector4(Color.R, Color.G, Color.B, Color.A) / 255f);
        var uvPositionEnd = UvOffset + UvSize;
        _model.Material.Shader.Constants!.SetValue<ITexCoordsConstantMember, Vector4>(new(UvOffset.X, uvPositionEnd.X,
            UvOffset.Y, uvPositionEnd.Y));
        _model.Render(target, 0f);
    }

    public static ICBMesh<UIShader.Vertex> CreateSharedMesh(GraphicsResources gfxRes)
        => new CBMesh<UIShader.Vertex>(gfxRes.GraphicsDevice, [
            new(new(-0.5f, 0.5f)),
            new(new(0.5f, 0.5f)),
            new(new(-0.5f, -0.5f)),
            new(new(0.5f, -0.5f)),
        ], [2, 1, 0, 1, 2, 3]);
}
