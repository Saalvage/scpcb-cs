using System.Numerics;
using Veldrid;

namespace SCPCB.Graphics.UserInterface.Primitives;

public class MouseFollowerElement<T> : InteractableUIElement<T> where T : UIElement {
    public MouseFollowerElement(T inner) : base(inner) { }

    public override void OnUpdate(Vector2 pos, InputSnapshot snapshot) {
        Inner.Position = pos;
    }
}
