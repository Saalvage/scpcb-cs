using scpcb.Graphics;

namespace scpcb.Scenes; 

public interface IScene : IDisposable {
    void Update(float delta);
    void Tick();
    void Render(RenderTarget target, float interp);
}
