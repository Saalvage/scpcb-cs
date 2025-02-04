using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Vertices;
using SCPCB.Graphics.Text;
using SCPCB.Graphics.Textures;

namespace SCPCB.Graphics.UserInterface.Primitives;

public class TextElement : UIElement {
    private readonly ICBShader<VPositionTexture2D> _shader;
    private readonly TextModel _text;

    public string Text {
        get => _text.Text;
        set => _text.Text = value;
    }

    public IReadOnlyList<Vector2> Offsets => _text.Offsets;

    public Vector2 Scale { get; set; } = Vector2.One;

    public override Vector2 PixelSize {
        get => _text.Dimensions * Scale;
        set => Scale = value / _text.Dimensions;
    }

    public TextElement(GraphicsResources gfxRes, Font font) {
        _shader = gfxRes.ShaderCache.GetShader<TextShader, VPositionTexture2D>();
        _text = new(gfxRes, font, _shader);
    }

    protected override void DrawInternal(IRenderTarget target, Vector2 position, float z) {
        _shader.Constants!.SetValue<IPositionConstantMember, Vector3>(new(position - _text.Dimensions / 2 * Scale, z + Z));
        // Scaling used to be part of the text mesh generation process, in favor of keeping that as simple as possible and because other
        // users of TextModel (in particular TextModel3D) do not need this functionality it's been turned into a constant member.
        // (This also fixes the bug where the scaling would not update if the text meshes weren't regenerated.)
        // We also flip the Y here, because the text mesh itself uses Y-up (which is reasonable for TextModel3D), but the UI uses Y-down.
        _shader.Constants!.SetValue<IScale2DConstantMember, Vector2>(Scale with { Y = -Scale.Y });
        foreach (var mesh in _text.Meshes) {
            mesh.Render(target, 0f);
        }
    }
}
