using scpcb.Graphics.Textures;

namespace scpcb.Entities;

public interface IRenderable : IPriorizableEntity {
    void Render(IRenderTarget target, float interp);
}
