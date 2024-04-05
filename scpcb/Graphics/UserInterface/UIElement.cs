using System.Numerics;
using scpcb.Graphics.Textures;
using scpcb.Utility;

namespace scpcb.Graphics.UserInterface;

public interface IUIElement {
    /// <summary>
    /// This might contain more children to which none of the mutating methods have access to.
    /// </summary>
    IReadOnlyList<IUIElement> Children { get; }
    void AddChild(IUIElement child);
    void AddChildren(IEnumerable<IUIElement> children);
    bool RemoveChild(IUIElement child);
    void ClearChildren();
    Vector2 Position { get; }
    Vector2 PixelSize { get; }
    float Z { get; }
    Alignment Alignment { get; }
    bool IsVisible { get; set; }
    void Draw(IRenderTarget target, IUIElement parent, Vector2 drawPos);
}

public class UIElement : IUIElement {
    /// <summary>
    /// The element has full control over these children.
    /// </summary>
    protected readonly List<IUIElement> _internalChildren = [];
    private readonly List<IUIElement> _publicChildren = [];
    public virtual IReadOnlyList<IUIElement> Children { get; }

    public void AddChild(IUIElement child) {
        _publicChildren.Add(child);
    }

    public void AddChildren(IEnumerable<IUIElement> children) {
        _publicChildren.AddRange(children);
    }

    public bool RemoveChild(IUIElement child) => _publicChildren.Remove(child);

    public void ClearChildren() {
        _publicChildren.Clear();
    }

    public Vector2 Position { get; set; }
    public float Z { get; set; }

    // TODO: Only this being virtual is weird.
    public virtual Vector2 PixelSize { get; set; }

    public Alignment Alignment { get; set; }
    public bool IsVisible { get; set; } = true;

    public UIElement() {
        Children = new ReadOnlyDuoList<IUIElement>(_publicChildren, _internalChildren);
    }

    protected virtual void DrawInternal(IRenderTarget target, Vector2 position) { }

    public void Draw(IRenderTarget target, IUIElement parent, Vector2 drawPos) {
        if (!IsVisible) {
            return;
        }

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

        drawPos.X += Position.X;
        drawPos.Y -= Position.Y; // Positive Y = down.

        DrawInternal(target, drawPos);
        foreach (var child in Children) {
            child.Draw(target, this, drawPos);
        }
    }
}
