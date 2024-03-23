using System.Numerics;
using scpcb.Graphics.Textures;

namespace scpcb.Graphics.UserInterface;

public interface IUIElement {
    IList<IUIElement> Children { get; }
    Vector2 Position { get; }
    Vector2 PixelSize { get; }
    float Z { get; }
    Alignment Alignment { get; }
    void Draw(IRenderTarget target, IUIElement parent, Vector2 drawPos);
}

public class UIElement : IUIElement {
    public IList<IUIElement> Children { get; } = [];

    public Vector2 Position { get; set; }
    public float Z { get; set; }

    public Vector2 PixelSize { get; set; }

    public Alignment Alignment { get; set; }

    protected virtual void DrawInternal(IRenderTarget target, Vector2 position) { }

    public void Draw(IRenderTarget target, IUIElement parent, Vector2 drawPos) {
        var direction = new Vector2(
            x: Alignment.Horizontality switch {
                Alignment.Horizontal.Left => 1,
                Alignment.Horizontal.Center => 0,
                Alignment.Horizontal.Right => -1,
            },
            y: Alignment.Verticality switch {
                Alignment.Vertical.Bottom => 1,
                Alignment.Vertical.Center => 0,
                Alignment.Vertical.Top => -1,
            }
        );

        var distanceToEdge = -parent.PixelSize / 2 + PixelSize / 2;
        drawPos += direction * distanceToEdge;

        drawPos += Position;

        DrawInternal(target, drawPos);
        foreach (var child in Children) {
            child.Draw(target, this, drawPos);
        }
    }
}
