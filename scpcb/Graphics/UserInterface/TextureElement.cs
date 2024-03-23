using System.Numerics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders;
using scpcb.Graphics.Textures;

namespace scpcb.Graphics.UserInterface;

public class TextureElement : UIElement {
    private readonly ICBModel _model;

    public TextureElement(UIManager manager, ICBTexture texture) {
        var gfxRes = manager.GraphicsResources;
        _model = new CBModel<UIShader.Vertex>(null,
            gfxRes.MaterialCache.GetMaterial<UIShader, UIShader.Vertex>([texture], [gfxRes.ClampAnisoSampler]),
            manager.UIMesh);
        PixelSize = new(texture.Width, texture.Height);
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position) {
        _model.Material.Shader.Constants!.SetValue<IPositionConstantMember, Vector3>(new(position, Z));
        _model.Material.Shader.Constants!.SetValue<IUIScaleConstantMember, Vector2>(PixelSize);
        target.Render(_model, 0f);
    }
}
