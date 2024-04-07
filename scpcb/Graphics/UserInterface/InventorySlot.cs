using System.Drawing;
using scpcb.Entities.Items;

namespace scpcb.Graphics.UserInterface;

public class InventorySlot : MenuFrame, IInteractableUIElement {
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
        : base(gfxRes, ui, outerXOff, innerXOff, yOff) {
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

    public void OnBeginHover() {
        _hoverBorder.IsVisible = true;
        _itemText.IsVisible = true;
    }

    public void OnEndHover() {
        _hoverBorder.IsVisible = false;
        _itemText.IsVisible = false;
    }

    public bool Hovering { get; set; }
    
    public void OnMouseDown() {
        _item?.OnUsed();
    }

    public void OnMouseUp() {

    }
}
