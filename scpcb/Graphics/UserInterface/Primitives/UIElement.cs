using System.Numerics;
using scpcb.Graphics.Textures;
using scpcb.Graphics.UserInterface.Utility;
using scpcb.Utility;

namespace scpcb.Graphics.UserInterface.Primitives;

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
    void Visit(IUIElement parent, Vector2 drawPos, Func<IUIElement, Vector2, bool> visitor, bool visitInvisible = false);
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

    // TODO: The design here needs to be revisited, the layout itself should be done by the element itself,
    // same goes for the drawing of the pre-positioned object, the "glue" part should be within the manager
    // to allow for e.g. grouping by texture and ordering by Z, utilizing the visitor.
    private Vector2 CalculateAbsolutePosition(IUIElement parent, Vector2 parentPos) {
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
        parentPos += direction * distanceToEdge;

        parentPos.X += Position.X;
        parentPos.Y -= Position.Y; // Positive Y = down.

        return parentPos;
    }
    
    public void Visit(IUIElement parent, Vector2 drawPos, Func<IUIElement, Vector2, bool> visitor, bool visitInvisible = false) {
        if (!visitInvisible && !IsVisible) {
            return;
        }

        var absPos = CalculateAbsolutePosition(parent, drawPos);

        if (visitor(this, absPos)) {
            foreach (var child in Children) {
                child.Visit(this, absPos, visitor, visitInvisible);
            }
        }
    }

    public void Draw(IRenderTarget target, IUIElement parent, Vector2 drawPos) {
        if (!IsVisible) {
            return;
        }

        var absPos = CalculateAbsolutePosition(parent, drawPos);
        DrawInternal(target, absPos);
        foreach (var child in Children) {
            child.Draw(target, this, absPos);
        }
    }

    // TODO: Store parent and add utilities for other directions.
    public float Left(IUIElement parent) {
        return Alignment.Horizontality switch {
            Alignment.Horizontal.Center => parent.PixelSize.X / 2 - PixelSize.X / 2 + Position.X,
            Alignment.Horizontal.Left => Position.X,
            Alignment.Horizontal.Right => parent.PixelSize.X - PixelSize.X + Position.X,
        };
    }
}
