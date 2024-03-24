using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.UserInterface;
using scpcb.Utility;

namespace scpcb;

public class HUD : Disposable, IEntity {
    private readonly UIManager _ui;

    private IUIElement? _singleHeadsUpItem;

    public HUD(UIManager ui) {
        _ui = ui;
    }

    public void SetItem(ICBTexture texture) {
        ClearItem();
        _singleHeadsUpItem = new TextureElement(_ui, texture) { Alignment = Alignment.Center, Z = 1 };
        _ui.Root.Children.Add(_singleHeadsUpItem);
    }

    public void ClearItem() {
        if (_singleHeadsUpItem != null) {
            _ui.Root.Children.Remove(_singleHeadsUpItem);
            _singleHeadsUpItem = null;
        }
    }

    protected override void DisposeImpl() {
        _ui.Dispose();
    }
}
