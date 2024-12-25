using SCPCB.Scenes;

namespace SCPCB.Entities;

public interface IEntity {
    void OnAdd(IScene scene) { }
    void OnRemove(IScene scene) { }
}
