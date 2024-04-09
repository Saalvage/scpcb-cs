using System.Drawing;
using System.Numerics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.UserInterface.Primitives;

namespace scpcb.Graphics.UserInterface.Composites;

public class BorderedImage : Border {
    public BorderedImage(GraphicsResources gfxRes, Vector2 dimensions, float thickness, Color color, ICBTexture texture) : base(gfxRes,
        dimensions, thickness, color) {
        AddChild(new TextureElement(gfxRes, texture) {
            Position = new(thickness),
            PixelSize = dimensions - new Vector2(2 * thickness),
        });
    }

    public BorderedImage(GraphicsResources gfxRes, float thickness, Color color, ICBTexture texture) : base(gfxRes,
        new Vector2(texture.Width, texture.Height) + new Vector2(thickness * 2), thickness, color) {
        AddChild(new TextureElement(gfxRes, texture) {
            Position = new(thickness),
        });
    }
}
