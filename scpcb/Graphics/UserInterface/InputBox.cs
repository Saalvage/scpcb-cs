using System.Numerics;
using scpcb.Graphics.UserInterface.Utility;
using Veldrid;

namespace scpcb.Graphics.UserInterface;

internal class InputBox : InteractableUIElement<MenuFrame> {
    private readonly TextInput _input;

    public InputBox(GraphicsResources gfxRes, UIManager ui, InputManager input, Font font) : base(new(gfxRes, ui, 0f, 0f, 0f)) {
        _internalChildren.Add(_input = new(gfxRes, input, font) {
            Alignment = Alignment.Center,
        });
        PixelSize = new(300, 50);
        _receiveMouseDownOutside = true;
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        _input.Selected = IsInElement(pos);
    }
}
