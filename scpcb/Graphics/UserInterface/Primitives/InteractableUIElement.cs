using System.Numerics;
using Veldrid;

namespace scpcb.Graphics.UserInterface.Primitives;

public interface IInteractableUIElement : IUIElement {
    void Update(Vector2 pos, InputSnapshot snapshot);
}

public class InteractableUIElement<TInner> : UIElement, IInteractableUIElement where TInner : UIElement {
    public TInner Inner { get; }

    public override Vector2 PixelSize { get => Inner.PixelSize; set => Inner.PixelSize = value; }

    private readonly Dictionary<MouseButton, bool> _downButtons = Enum.GetValues<MouseButton>().ToDictionary(x => x, _ => false);

    protected bool _hovering { get; private set; } = false;

    protected bool _receiveMouseDownOutside = false;

    public InteractableUIElement(TInner inner) {
        Inner = inner;
        _internalChildren.Add(inner);
    }

    public void Update(Vector2 pos, InputSnapshot snapshot) {
        var mouseInElem = IsInElement(snapshot.MousePosition - pos);
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
                    if (mouseInElem || _receiveMouseDownOutside) {
                        OnMouseDown(mb, snapshot.MousePosition - pos);
                        _downButtons[mb] = true;
                    }
                } else {
                    OnMouseUp(mb, snapshot.MousePosition - pos);
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

        OnUpdate(snapshot.MousePosition - pos, snapshot);
    }

    protected bool IsInElement(Vector2 pos)
        => pos.X >= 0 && pos.X <= PixelSize.X
        && pos.Y >= 0 && pos.Y <= PixelSize.Y;

    public virtual void OnUpdate(Vector2 pos, InputSnapshot snapshot) { }

    protected virtual void OnBeginHover() { }
    protected virtual void OnEndHover() { }

    protected virtual void OnMouseDown(MouseButton button, Vector2 pos) { }
    protected virtual void OnMouseUp(MouseButton button, Vector2 pos) { }

    protected virtual void OnTextInput(char ch) { }
    protected virtual void OnKeyPressed(Key key, ModifierKeys modifiers) { }
}
