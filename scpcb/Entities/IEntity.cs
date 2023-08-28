using scpcb.Scenes;

namespace scpcb.Entities;

public interface IEntity {
    void OnAdd(IScene scene) { }
    void OnRemove(IScene scene) { }
}
