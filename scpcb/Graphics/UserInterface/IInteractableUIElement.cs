namespace scpcb.Graphics.UserInterface;

// TODO: Should be obvious, but this needs to be revisited, the primary question is probably where to store the state,
// since it needs to be modified externally (by the ui manager), but should not be publicly exposed.
// Currently leaning towards a transparent wrapper implementation of this which gets the events fed directly
// and provides the utility currently contained herein for derivatives.
public interface IInteractableUIElement : IUIElement {
    void OnBeginHover() { }
    void OnEndHover() { }
    bool Hovering { get; set; }
    public void OnMouseDown() { }
    public void OnMouseUp() { }
    public void OnClicked() { }
}
