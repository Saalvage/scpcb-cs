using System.Numerics;
using SCPCB.Graphics.Primitives;

namespace SCPCB.Graphics.UserInterface.Primitives;

public class TiledTextureElement : TextureElement {
    private readonly ICBTexture _texture;

    public TiledTextureElement(GraphicsResources gfxRes, ICBTexture texture) : base(gfxRes, texture, true) {
        _texture = texture;
    }

    private Vector2 _pixelSize;
    public override Vector2 PixelSize {
        get => _pixelSize;
        set {
            _pixelSize = value;
            if (_texture != null) {
                UvSize = _pixelSize / new Vector2(_texture.Width, _texture.Height);
            }
        }
    }
}
