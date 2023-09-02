using scpcb.Graphics.Textures;

namespace scpcb.Entities;

public interface IRenderable : IComparable<IRenderable>, IEntity {
    void Render(IRenderTarget target, float interp);
    /// <summary>
    /// Static priority. Lower is rendered first. This value is only considered during insertion.
    /// </summary>
    int Priority => 0;

    int IComparable<IRenderable>.CompareTo(IRenderable? other) => Priority.CompareTo(other?.Priority);
}
