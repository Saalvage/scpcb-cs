using scpcb.Entities;
using scpcb.Graphics;
using scpcb.Utility;

namespace scpcb.Scenes; 

public class BaseScene : Disposable, IScene {
    private readonly List<IEntity> _entities = new();
    private readonly List<IUpdatable> _updatables = new();
    private readonly List<ITickable> _tickables = new();
    private readonly List<IRenderable> _renderables = new();

    private readonly List<IEntity> _entitiesToAdd = new();
    private readonly List<ValueTuple<IEntity, bool>> _entitiesToRemove = new();

    public IEnumerable<IEntity> Entities => _entities;

    public event Action<IEntity> OnAddEntity;
    public event Action<IEntity> OnRemoveEntity;

    public void AddEntity(IEntity entity) {
        _entitiesToAdd.Add(entity);
    }

    public void AddEntities(IEnumerable<IEntity> entities) {
        _entitiesToAdd.AddRange(entities);
    }

    /// <summary>
    /// Removes and disposes.
    /// </summary>
    /// <param name="entity"></param>
    public void RemoveEntity(IEntity entity) {
        _entitiesToRemove.Add(new(entity, true));
    }

    /// <summary>
    /// Removes without disposing, moving it to the other scene if provided.
    /// </summary>
    /// <param name="entity"></param>
    public void MoveEntity(IEntity entity, BaseScene? other = null) {
        _entitiesToRemove.Add(new(entity, false));
        other?.AddEntity(entity);
    }

    private void HandleAddEntity(IEntity e) {
        e.OnAdd(this);
        OnAddEntity?.Invoke(e);
        _entities.Add(e);
        if (e is IUpdatable u) { _updatables.Add(u); }
        if (e is ITickable t) { _tickables.Add(t); }
        if (e is IRenderable r) { _renderables.Add(r); }
        if (e is IEntityHolder h) {
            foreach (var he in h.Entities) {
                HandleAddEntity(he);
            }
        }
    }

    private void HandleRemoveEntity(IEntity e, bool shouldDispose) {
        e.OnRemove(this);
        OnRemoveEntity?.Invoke(e);
        _entities.Remove(e);
        if (e is IUpdatable u) { _updatables.Remove(u); }
        if (e is ITickable t) { _tickables.Remove(t); }
        if (e is IRenderable r) { _renderables.Remove(r); }
        if (e is IEntityHolder h) {
            foreach (var he in h.Entities) {
                HandleRemoveEntity(he, shouldDispose);
            }
        }
        if (shouldDispose && e is IDisposable d) { d.Dispose(); }
    }

    private void DealWithEntityBuffers() {
        foreach (var e in _entitiesToAdd) {
            HandleAddEntity(e);
        }
        _entitiesToAdd.Clear();

        foreach (var (e, shouldDispose) in _entitiesToRemove) {
            HandleRemoveEntity(e, shouldDispose);
        }
        _entitiesToRemove.Clear();
    }

    // TODO: Consider: Events instead of virtual?
    public virtual void Update(float delta) {
        foreach (var u in _updatables) {
            u.Update(delta);
        }

        DealWithEntityBuffers();
    }

    public virtual void Tick() {
        foreach (var t in _tickables) {
            t.Tick();
        }

        DealWithEntityBuffers();
    }

    public virtual void Render(RenderTarget target, float interp) {
        foreach (var r in _renderables) {
            r.Render(target, interp);
        }

        // Don't deal with buffers here, rendering should NOT affect them.
    }

    protected override void DisposeImpl() {
        foreach (var e in _entities) {
            RemoveEntity(e);
        }
        DealWithEntityBuffers();
    }
}
