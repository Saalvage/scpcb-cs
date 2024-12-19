using System.Drawing;
using scpcb.Graphics.UserInterface.Primitives;

namespace scpcb.Graphics.UserInterface.Composites;

public class MapTile : UIElement {
    private Border _border;
    private TextureElement? _tile;

    public TextureElement? Tile {
        get => _tile;
        set {
            if (_tile != null) {
                _internalChildren.Remove(_tile);
            }
            _tile = value;
            if (_tile != null) {
                _internalChildren.Add(_tile);
            }
            _border.IsVisible = _tile == null;
        }
    }

    public MapTile(GraphicsResources gfxRes, float size) {
        _border = new(gfxRes, new(size), 1, Color.White);
        _internalChildren.Add(_border);
    }
}
