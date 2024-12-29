using System.Numerics;
using SCPCB.Graphics.UserInterface.Primitives;
using SCPCB.Graphics.UserInterface.Utility;
using Veldrid;

namespace SCPCB.Graphics.UserInterface.Composites;

internal class InputBox : InteractableUIElement<MenuFrame> {
    private readonly UIManager _ui;

    private bool _selecting = false;

    public TextInput Input { get; }

    public InputBox(GraphicsResources gfxRes, UIManager ui, InputManager input, Font font) : base(new(gfxRes, ui, 0f, 0f, 0f)) {
        _ui = ui;
        _internalChildren.Add(Input = new(gfxRes, input, font) {
            Alignment = Alignment.Center,
        });
        PixelSize = new(300, 50);
        _receiveMouseDownOutside = true;
    }

    private int GetCaretIndex(Vector2 pos) {
        var textElement = Input.Inner;
        var mouseInText = pos.X - Input.Left(this);
        for (int i = 0; i < textElement.Offsets.Count - 1; i++) {
            // Average of (middle between) two offsets is our divider.
            if ((textElement.Offsets[i].X + textElement.Offsets[i + 1].X) / 2 > mouseInText) {
                return i;
            }
        }
        return textElement.Text.Length;
    }

    protected override void OnMouseDown(MouseButton button, Vector2 pos) {
        if (Input.Selected = _selecting = IsInElement(pos)) {
            Input.Caret = GetCaretIndex(pos);
        }
    }

    public override void OnUpdate(Vector2 pos, InputSnapshot snapshot) {
        if (_selecting) {
            Input.CaretWanderer = GetCaretIndex(pos);
        }
    }

    protected override void OnMouseUp(MouseButton button, Vector2 pos) {
        _selecting = false;
    }

    protected override void OnBeginHover(InputSnapshot snapshot) {
        _ui.SetCursorStyle(UIManager.CursorStyle.Text);
    }

    protected override void OnEndHover(InputSnapshot snapshot) {
        _ui.SetCursorStyle(UIManager.CursorStyle.Default);
    }
}
