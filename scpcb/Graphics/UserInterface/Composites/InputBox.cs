using System.Numerics;
using scpcb.Graphics.UserInterface.Primitives;
using scpcb.Graphics.UserInterface.Utility;
using Veldrid;

namespace scpcb.Graphics.UserInterface.Composites;

internal class InputBox : InteractableUIElement<MenuFrame> {
    public TextInput Input { get; }

    public InputBox(GraphicsResources gfxRes, UIManager ui, InputManager input, Font font) : base(new(gfxRes, ui, 0f, 0f, 0f)) {
        _internalChildren.Add(Input = new(gfxRes, input, font) {
            Alignment = Alignment.Center,
        });
        PixelSize = new(300, 50);
        _receiveMouseDownOutside = true;
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        Input.Selected = IsInElement(pos);
    }
}
