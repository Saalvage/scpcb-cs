using scpcb.Graphics.Textures;

namespace scpcb.Entities;

public interface IRenderable : IEntity {
    public void Render(IRenderTarget target, float interp);
}
