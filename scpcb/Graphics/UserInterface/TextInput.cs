using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using scpcb.Graphics.UserInterface.Utility;
using Veldrid;
using Veldrid.Sdl2;

namespace scpcb.Graphics.UserInterface;

// TODO:
// - Interaction with ctrl.
// - Selecting by clicking/dragging.
public class TextInput : InteractableUIElement<TextElement> {
    private readonly InputManager _input;

    private readonly MementoManager _mementoManager = new();

    private const int CARET_WIDTH = 2;

    private int _caret = 0;
    private int _caretWanderer = 0;

    private int CaretLeft() => Math.Min(_caret, _caretWanderer);
    private int CaretRight() => Math.Max(_caret, _caretWanderer);

    private readonly TextureElement _caretElem;

    private bool _selected;
    public bool Selected {
        get => _selected;
        set {
            _selected = value;
            if (_caret != _caretWanderer) {
                _caretElem.Color = _caretSelectionColor;
            } else {
                _caretElem.IsVisible = _selected;
            }
        }
    }

    private Color _caretSelectionColor => Color.FromArgb(128, _selected ? Color.White : Color.Gray);

    public TextInput(GraphicsResources gfxRes, InputManager input, Font font) : base(new(gfxRes, font)) {
        _input = input;
        _internalChildren.Add(_caretElem = new(gfxRes, gfxRes.TextureCache.GetTexture(Color.White)) {
            PixelSize = new(CARET_WIDTH, font.VerticalAdvance),
            Alignment = Alignment.CenterLeft,
        });
        Selected = false;
        RepositionCarets();
    }

    private void RepositionCarets() {
        Debug.Assert(_caret >= 0 && _caret <= Inner.Text.Length);
        Debug.Assert(_caretWanderer >= 0 && _caretWanderer <= Inner.Text.Length);

        if (_caret == _caretWanderer) {
            _caretElem.PixelSize = _caretElem.PixelSize with { X = CARET_WIDTH };
            _caretElem.Color = Color.White;
        } else {
            _caretElem.PixelSize = _caretElem.PixelSize with { X = Inner.Offsets[CaretRight()].X - Inner.Offsets[CaretLeft()].X };
            _caretElem.Color = _caretSelectionColor;
        }
        _caretElem.Position = Inner.Offsets[CaretLeft()] - new Vector2(CARET_WIDTH / 2f, 0f);
    }

    protected override void OnTextInput(char ch) {
        if (!_selected) {
            return;
        }

        InsertInSelection(ch.ToString());
        RepositionCarets();
    }

    protected override void OnKeyPressed(Key key, ModifierKeys modifiers) {
        if (!_selected) {
            return;
        }

        switch (key) {
            case Key.Back:
                if (_caret != _caretWanderer) {
                    DeleteSelectedText();
                } else if (_caret != 0) {
                    _mementoManager.Submit(NextLeft(),
                        new(Inner.Text[_caret - 1].ToString(), _caret - 1, false, _caret, _caretWanderer, false));
                    Inner.Text = Inner.Text[..(_caret - 1)] + Inner.Text[_caret..];
                    _caret = _caretWanderer = NextLeft();
                }
                break;
            case Key.Left:
                if (_input.IsKeyDown(Key.ShiftLeft)) {
                    _caretWanderer = NextLeft();
                    break;
                }

                if (_caret != _caretWanderer) {
                    _caretWanderer = _caret = CaretLeft();
                } else {
                    _caret = _caretWanderer = NextLeft();
                }
                break;
            case Key.Right:
                if (_input.IsKeyDown(Key.ShiftLeft)) {
                    _caretWanderer = NextRight();
                    break;
                }

                if (_caret != _caretWanderer) {
                    _caret = _caretWanderer = CaretRight();
                } else {
                    _caret = _caretWanderer = NextRight();
                }
                break;
            case Key.Up:
                if (!modifiers.HasFlag(ModifierKeys.Shift)) {
                    _caret = 0;
                }
                _caretWanderer = 0;
                break;
            case Key.Down:
                if (!modifiers.HasFlag(ModifierKeys.Shift)) {
                    _caret = Inner.Text.Length;
                }
                _caretWanderer = Inner.Text.Length;
                break;
        }

        if ((modifiers & ModifierKeys.Control) != 0) {
            switch (key) {
                case Key.V:
                    var clipboard = Sdl2Native.SDL_GetClipboardText();
                    InsertInSelection(clipboard);
                    break;
                case Key.C:
                    CopySelectedTextToClipboard();
                    break;
                case Key.X:
                    CopySelectedTextToClipboard();
                    DeleteSelectedText();
                    break;
                case Key.A:
                    _caret = 0;
                    _caretWanderer = Inner.Text.Length;
                    break;
                // TODO: Veldrid throws away the virtual keycodes, which we need.
                case Key.Z:
                    (Inner.Text, _caret, _caretWanderer) = _mementoManager.Undo(Inner.Text);
                    break;
                case Key.Y:
                    (Inner.Text, _caret, _caretWanderer) = _mementoManager.Redo(Inner.Text);
                    break;
            }
        }

        RepositionCarets();

        int NextLeft() => Math.Max(0, _caretWanderer - 1);
        int NextRight() => Math.Min(Inner.Text.Length, _caretWanderer + 1);

        void CopySelectedTextToClipboard() => Sdl2Native.SDL_SetClipboardText(Inner.Text[CaretLeft()..CaretRight()]);

        void DeleteSelectedText() {
            _mementoManager.Submit(CaretLeft(),
                new(Inner.Text[CaretLeft()..CaretRight()], CaretLeft(), false, _caret, _caretWanderer, false));
            Inner.Text = Inner.Text[..CaretLeft()] + Inner.Text[CaretRight()..];
            _caret = _caretWanderer = CaretLeft();
        }
    }

    private void InsertInSelection(string str) {
        var linked = _caret != _caretWanderer;
        var newCaret = CaretLeft() + str.Length;
        if (linked) {
            _mementoManager.Submit(newCaret, new(Inner.Text[CaretLeft()..CaretRight()], CaretLeft(),
                false, _caret, _caretWanderer, true));
        }
        _mementoManager.Submit(newCaret, new(str, CaretLeft(), true,
            _caret, _caretWanderer, linked));
        Inner.Text = Inner.Text[..CaretLeft()] + str + Inner.Text[CaretRight()..];
        _caret = _caretWanderer = newCaret;
    }
}
