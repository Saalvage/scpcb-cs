using System.Numerics;
using Veldrid;

namespace scpcb.Graphics.UserInterface;

public interface IInteractableUIElement : IUIElement {
    void Update(Vector2 pos, InputSnapshot snapshot);
}

public class InteractableUIElement<TInner> : UIElement, IInteractableUIElement where TInner : UIElement {
    public TInner Inner { get; }

    public override Vector2 PixelSize { get => Inner.PixelSize; set => Inner.PixelSize = value; }

    private readonly Dictionary<MouseButton, bool> _downButtons = Enum.GetValues<MouseButton>().ToDictionary(x => x, _ => false);

    protected bool _hovering { get; private set; } = false;

    public InteractableUIElement(TInner inner) {
        Inner = inner;
        _internalChildren.Add(inner);
    }

    public void Update(Vector2 pos, InputSnapshot snapshot) {
        var mouseInElem = IsInRect(pos, snapshot.MousePosition);
        if (_hovering != mouseInElem) {
            if (mouseInElem) {
                OnBeginHover();
            } else {
                OnEndHover();
            }
            _hovering = mouseInElem;
        }

        foreach (var mb in Enum.GetValues<MouseButton>()) {
            var newDown = snapshot.IsMouseDown(mb);
            if (_downButtons[mb] != newDown) {
                // Note the semantics here: mouse down is only reported if the mouse is on the element
                // while mouse up is always reported if the mouse was previously downed on the element.
                if (newDown) {
                    if (mouseInElem) {
                        OnMouseDown(mb);
                        _downButtons[mb] = true;
                    }
                } else {
                    OnMouseUp(mb);
                    _downButtons[mb] = false;
                }
                
            }
        }

        foreach (var ch in snapshot.KeyCharPresses) {
            OnTextInput(ch);
        }

        foreach (var ev in snapshot.KeyEvents) {
            if (ev.Down) {
                OnKeyPressed(ev.Key, ev.Modifiers);
            }
        }
    }

    private bool IsInRect(Vector2 myPos, Vector2 otherPos)
        => myPos.X <= otherPos.X && myPos.X + PixelSize.X >= otherPos.X
        && myPos.Y <= otherPos.Y && myPos.Y + PixelSize.Y >= otherPos.Y;

    protected virtual void OnBeginHover() { }
    protected virtual void OnEndHover() { }

    protected virtual void OnMouseDown(MouseButton button) { }
    protected virtual void OnMouseUp(MouseButton button) { }

    protected virtual void OnTextInput(char ch) { }
    protected virtual void OnKeyPressed(Key key, ModifierKeys modifiers) { }
}
