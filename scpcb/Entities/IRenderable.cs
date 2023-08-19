using scpcb.Graphics;

namespace scpcb.Entities; 

public interface IRenderable : IEntity {
    public void Render(RenderTarget target, float interp);
}
