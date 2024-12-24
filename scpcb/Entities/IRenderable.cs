using SCPCB.Graphics.Textures;

namespace SCPCB.Entities;

public interface IRenderable : IPriorizableEntity {
    void Render(IRenderTarget target, float interp);
}
