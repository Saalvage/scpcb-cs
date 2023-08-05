using scpcb.Graphics;

namespace scpcb; 

public interface IScene : IDisposable {
    void Update(double delta);
    void Tick();
    void Render(RenderTarget target, float interp);
}
