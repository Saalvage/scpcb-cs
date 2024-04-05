using System.Numerics;
using scpcb.Graphics.Primitives;

namespace scpcb.Graphics.UserInterface;

public class TiledTextureElement : UIElement {
    private readonly TextureElement _internal;
    private readonly ICBTexture _tex;

    public TiledTextureElement(GraphicsResources gfxRes, ICBTexture tex) {
        _tex = tex;
        _internal = new(gfxRes, tex);
        _internalChildren.Add(_internal);
    }

    public override Vector2 PixelSize {
        get => _internal.PixelSize;
        set {
            _internal.PixelSize = value;
            _internal.UvSize = _internal.PixelSize / new Vector2(_tex.Width, _tex.Height);
        }
    }
}
