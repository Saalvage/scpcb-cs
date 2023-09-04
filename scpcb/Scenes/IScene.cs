using scpcb.Entities;
using scpcb.Graphics.Textures;

namespace scpcb.Scenes; 

public interface IScene : IDisposable {
    IEnumerable<IEntity> Entities { get; }

    IReadOnlyList<T> GetEntitiesOfType<T>() where T : IEntity;

    void Update(float delta);
    void Tick();
    void Prerender(float interp);
    void Render(IRenderTarget target, float interp);

    event Action<IEntity> OnAddEntity;
    event Action<IEntity> OnRemoveEntity;

    void OnEnter() { }
    void OnLeave() { }
}
