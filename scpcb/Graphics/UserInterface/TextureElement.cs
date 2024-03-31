using System.Drawing;
using System.Numerics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Textures;
using scpcb.Graphics.Caches;

namespace scpcb.Graphics.UserInterface;

public class TextureElement : UIElement, ISharedMeshProvider<TextureElement, UIShader.Vertex>, IColorizableElement {
    private readonly ICBModel _model;

    public Color Color { get; set; } = Color.White;

    public TextureElement(GraphicsResources gfxRes, ICBTexture texture) {
        _model = new CBModel<UIShader.Vertex>(null,
            gfxRes.MaterialCache.GetMaterial<UIShader, UIShader.Vertex>([texture], [gfxRes.ClampAnisoSampler]),
            gfxRes.MeshCache.GetMesh<TextureElement, UIShader.Vertex>());
        PixelSize = new(texture.Width, texture.Height);
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position) {
        _model.Material.Shader.Constants!.SetValue<IPositionConstantMember, Vector3>(new(position, Z));
        _model.Material.Shader.Constants!.SetValue<IUIScaleConstantMember, Vector2>(PixelSize);
        _model.Material.Shader.Constants!.SetValue<IColorConstantMember, Vector3>(new Vector3(Color.R, Color.G, Color.B) / 255f);
        _model.Render(target, 0f);
    }

    public static ICBMesh<UIShader.Vertex> CreateSharedMesh(GraphicsResources gfxRes)
        => new CBMesh<UIShader.Vertex>(gfxRes.GraphicsDevice, [
            new(new(-0.5f, -0.5f), new(0, 1)),
            new(new(0.5f, -0.5f), new(1, 1)),
            new(new(-0.5f, 0.5f), new(0, 0)),
            new(new(0.5f, 0.5f), new(1, 0)),
        ], [0, 1, 2, 3, 2, 1]);
}
