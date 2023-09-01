using scpcb.Entities;
using scpcb.Graphics.Textures;

namespace scpcb.Scenes; 

public interface IScene : IDisposable {
    IEnumerable<IEntity> Entities { get; }

    void Update(float delta);
    void Tick();
    void Render(IRenderTarget target, float interp);

    void OnEnter() { }
    void OnLeave() { }
}
