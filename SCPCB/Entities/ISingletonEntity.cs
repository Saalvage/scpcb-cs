using SCPCB.Scenes;

namespace SCPCB.Entities;

public interface ISingletonEntity<T> : IEntity where T : ISingletonEntity<T> {
    void IEntity.OnAdd(IScene scene) {
        if (scene.GetEntitiesOfType<T>().Skip(1).Any()) {
            throw new InvalidOperationException($"Singleton of type {typeof(T).Name} already exists in scene!");
        }
    }
}
