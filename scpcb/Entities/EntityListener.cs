using SCPCB.Scenes;
using SCPCB.Utility;

namespace SCPCB.Entities;

/// <summary>
/// Helper class that automatically takes care of (un-)subscribing to the events.
/// </summary>
public abstract class EntityListener : Disposable, IEntity {
    protected readonly IScene _scene;

    protected EntityListener(IScene scene) {
        _scene = scene;

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
