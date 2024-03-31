using System.Drawing;
using System.Numerics;
using scpcb.Utility;

namespace scpcb.Graphics.UserInterface;

public class Border : UIElement, IColorizableElement {
    private readonly TextureElement[] _internalChildren;

    public Border(GraphicsResources gfxRes, Vector2 dimensions, float thickness, Color color) {
        PixelSize = dimensions;
        var texture = gfxRes.TextureCache.GetTexture(color);
        _internalChildren = [
            new(gfxRes, texture) { Alignment = Alignment.CenterLeft, PixelSize = new(thickness, dimensions.Y) },
            new(gfxRes, texture) { Alignment = Alignment.CenterRight, PixelSize = new(thickness, dimensions.Y) },
            new(gfxRes, texture) { Alignment = Alignment.TopCenter, PixelSize = new(dimensions.X - 2 * thickness, thickness) },
            new(gfxRes, texture) { Alignment = Alignment.BottomCenter, PixelSize = new(dimensions.X - 2 * thickness, thickness) },
        ];
        Children.AddRange(_internalChildren);
    }

    public Color Color {
        get => _internalChildren[0].Color;
        set {
            foreach (var child in _internalChildren) {
                child.Color = value;
            }
        }
    }
}
