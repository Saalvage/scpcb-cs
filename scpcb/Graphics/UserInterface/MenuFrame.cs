using System.Numerics;

namespace scpcb.Graphics.UserInterface;

public class MenuFrame : UIElement {
    protected readonly UIManager _ui;

    private readonly TiledTextureElement _outer;
    private readonly TiledTextureElement _inner;

    // The offset system is really weird, taken straight from the og!
    public MenuFrame(GraphicsResources gfxRes, UIManager ui, float outerXOff, float innerXOff, float yOff) {
        _ui = ui;
        var outerTex = gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/menuwhite.jpg");
        var innerTex = gfxRes.TextureCache.GetTexture("Assets/Textures/HUD/menublack.jpg");
        _outer = new(gfxRes, outerTex);
        _inner = new(gfxRes, innerTex) {
            Alignment = Alignment.Center,
        };
        _outer.UvOffset = new(outerXOff / outerTex.Width, yOff / outerTex.Height);
        _inner.UvOffset = new(innerXOff / innerTex.Width, yOff / innerTex.Height);
        _internalChildren.Add(_outer);
        _internalChildren.Add(_inner);
    }

    private Vector2 _pixelSize;
    public override Vector2 PixelSize {
        get => _pixelSize;
        set {
            _inner.PixelSize = value - new Vector2((int)(6 * _ui.MenuScale));
            _outer.PixelSize = value;
            _pixelSize = value;
        }
    }
}
