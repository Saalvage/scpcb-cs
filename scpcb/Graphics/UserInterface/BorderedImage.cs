using System.Drawing;
using System.Numerics;
using scpcb.Graphics.Primitives;

namespace scpcb.Graphics.UserInterface;

public class BorderedImage : Border {
    public BorderedImage(GraphicsResources gfxRes, Vector2 dimensions, float thickness, Color color, ICBTexture texture) : base(gfxRes,
        dimensions, thickness, color) {
        Children.Add(new TextureElement(gfxRes, texture) {
            Position = new(thickness),
            PixelSize = dimensions - new Vector2(2 * thickness),
        });
    }

    public BorderedImage(GraphicsResources gfxRes, float thickness, Color color, ICBTexture texture) : base(gfxRes,
        new Vector2(texture.Width, texture.Height) + new Vector2(thickness * 2), thickness, color) {
        Children.Add(new TextureElement(gfxRes, texture) {
            Position = new(thickness),
        });
    }
}
