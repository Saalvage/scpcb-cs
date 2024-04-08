using System.Numerics;

namespace scpcb.Graphics.UserInterface;

public interface IInteractableUIElement : IUIElement {
    void MouseMove(Vector2 pos, Vector2 mousePos);
}

public class InteractableUIElement : UIElement, IInteractableUIElement {
    public UIElement Inner { get; }

    public override Vector2 PixelSize { get => Inner.PixelSize; set => Inner.PixelSize = value; }

    protected bool _hovering { get; private set; } = false;

    public InteractableUIElement(UIElement inner) {
        Inner = inner;
        _internalChildren.Add(inner);
    }

    public void MouseMove(Vector2 pos, Vector2 mousePos) {
        var newHovering = pos.X <= mousePos.X && pos.X + PixelSize.X >= mousePos.X
                                              && pos.Y <= mousePos.Y && pos.Y + PixelSize.Y >= mousePos.Y;
        if (_hovering != newHovering) {
            if (newHovering) {
                OnBeginHover();
            } else {
                OnEndHover();
            }
        }

        _hovering = newHovering;
    }

    protected virtual void OnBeginHover() { }
    protected virtual void OnEndHover() { }

    public virtual void MouseDown(Vector2 pos, Vector2 mousePos) { }
    public virtual void MouseUp(Vector2 pos, Vector2 mousePos) { }
}
