using System.Numerics;

namespace scpcb.Graphics.UserInterface;

internal class TexturedBorder : UIElement {
    private readonly TiledTextureElement _outer;
    private readonly TiledTextureElement _inner;

    public TexturedBorder(GraphicsResources gfxRes) {
        _outer = new(gfxRes, gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/menuwhite.jpg"));
        _inner = new(gfxRes, gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/menublack.jpg")) {
            Alignment = Alignment.Center,
        };
        _internalChildren.Add(_outer);
        _internalChildren.Add(_inner);
    }

    private Vector2 _pixelSize;
    public override Vector2 PixelSize {
        get => _pixelSize;
        set {
            _inner.PixelSize = value - new Vector2(4);
            _outer.PixelSize = value;
            _pixelSize = value;
        }
    }
}
