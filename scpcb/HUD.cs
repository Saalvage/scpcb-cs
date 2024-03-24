using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.UserInterface;

namespace scpcb;

public class HUD : IEntity {
    private readonly UIManager _ui;

    private TextureElement? _singleHeadsUpItem;

    public HUD(UIManager ui) {
        _ui = ui;
    }

    public void SetItem(ICBTexture texture) {
        ClearItem();
        _singleHeadsUpItem = new(_ui.GraphicsResources, texture) { Alignment = Alignment.Center, Z = 1 };
        _singleHeadsUpItem.PixelSize *= _ui.MenuScale;
        _ui.Root.Children.Add(_singleHeadsUpItem);
    }

    public void ClearItem() {
        if (_singleHeadsUpItem != null) {
            _ui.Root.Children.Remove(_singleHeadsUpItem);
            _singleHeadsUpItem = null;
        }
    }
}
