using scpcb.Scenes;
using scpcb.Utility;

namespace scpcb.Entities;

/// <summary>
/// Helper class that automatically takes care of (un-)subscribing to the events.
/// </summary>
public abstract class EntityListener : Disposable, IEntity {
    protected readonly IScene _scene;

    /// <param name="wantInitial">Whether the entities already in the scene should be passed to the OnAddEntity method.</param>
    protected EntityListener(IScene scene, bool wantInitial) {
        _scene = scene;

        if (wantInitial) {
            foreach (var entity in _scene.Entities) {
                OnAddEntity(entity);
            }
        }

        _scene.OnAddEntity += OnAddEntity;
        _scene.OnRemoveEntity += OnRemoveEntity;
    }

    protected abstract void OnAddEntity(IEntity entity);
    
    protected abstract void OnRemoveEntity(IEntity entity);

    protected override void DisposeImpl() {
        _scene.OnAddEntity -= OnAddEntity;
        _scene.OnRemoveEntity -= OnRemoveEntity;
    }
}
