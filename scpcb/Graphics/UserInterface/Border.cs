using System.Drawing;
using System.Numerics;

namespace scpcb.Graphics.UserInterface;

public class Border : UIElement {
    public Border(GraphicsResources gfxRes, Vector2 dimensions, float thickness, Color color) {
        PixelSize = dimensions;
        var texture = gfxRes.TextureCache.GetTexture(color);
        Children.Add(new TextureElement(gfxRes, texture) { Alignment = Alignment.CenterLeft, PixelSize = new(thickness, dimensions.Y) });
        Children.Add(new TextureElement(gfxRes, texture) { Alignment = Alignment.CenterRight, PixelSize = new(thickness, dimensions.Y) });
        Children.Add(new TextureElement(gfxRes, texture) { Alignment = Alignment.TopCenter, PixelSize = new(dimensions.X - 2 * thickness, thickness) });
        Children.Add(new TextureElement(gfxRes, texture) { Alignment = Alignment.BottomCenter, PixelSize = new(dimensions.X - 2 * thickness, thickness) });
    }
}
