using System.Drawing;
using System.Numerics;
using scpcb.Entities.Items;
using scpcb.Graphics.UserInterface.Utility;
using Veldrid;

namespace scpcb.Graphics.UserInterface;

public class InventorySlot : InteractableUIElement<MenuFrame> {
    private readonly UIManager _ui;
    
    private readonly IUIElement _hoverBorder;

    private IItem? _item;
    private IUIElement? _itemElement;
    private TextElement _itemText;

    public IItem? Item {
        get => _item;
        set {
            if (_item == value) {
                return;
            }

            _item = value;
            if (_itemElement != null) {
                _internalChildren.Remove(_itemElement);
                _itemElement = null;
            }

            if (_item != null) {
                _internalChildren.Add(_itemElement = new TextureElement(_ui.GraphicsResources, _item.InventoryIcon) {
                    PixelSize = new(64),
                    Z = 10,
                    Alignment = Alignment.Center,
                });
            }

            _itemText.Text = _item?.DisplayName ?? "";
        }
    }

    public InventorySlot(GraphicsResources gfxRes, UIManager ui, float size, float outerXOff, float innerXOff, float yOff)
        : base(new(gfxRes, ui, outerXOff, innerXOff, yOff)) {
        _ui = ui;
        PixelSize = new(size);
        _internalChildren.Add(_hoverBorder = new Border(gfxRes, new(size + 2), 1, Color.Red) {
            Alignment = Alignment.Center,
            IsVisible = false,
        });
        _internalChildren.Add(_itemText = new(gfxRes, gfxRes.FontCache.GetFont("Assets/Fonts/Courier New.ttf", 19)) {
            Alignment = Alignment.BottomCenter,
            Position = new(0, 30),
            IsVisible = false,
        });
    }

    protected override void OnBeginHover() {
        _hoverBorder.IsVisible = true;
        _itemText.IsVisible = true;
    }

    protected override void OnEndHover() {
        _hoverBorder.IsVisible = false;
        _itemText.IsVisible = false;
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        _item?.OnUsed();
    }
}
