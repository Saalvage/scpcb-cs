using System.Collections;
using scpcb.Entities;
using scpcb.Graphics.Textures;
using scpcb.Utility;

namespace scpcb.Scenes;

public class BaseScene : Disposable, IScene {
    // This fucking sucks, all because BinarySearch is not an extension method for some reason??
    private interface IListWrapper {
        IList List { get; }
        int BinarySearch(IPriorizableEntity entity);
    }

    private record ListWrapper<T>(List<T> List) : IListWrapper {
        IList IListWrapper.List => List;

        public int BinarySearch(IPriorizableEntity entity)
            => List.BinarySearch((T)entity);
    }

    private readonly List<IEntity> _entities = [];
    private readonly Dictionary<Type, IListWrapper> _entitiesByType = [];

    private readonly List<IEntity> _entitiesToAdd = [];
    private readonly List<(IEntity, bool ShouldDispose)> _entitiesToRemove = [];

    public IEnumerable<IEntity> Entities => _entities;

    public event Action<IEntity> OnAddEntity;
    public event Action<IEntity> OnRemoveEntity;

    public void AddEntity(IEntity entity) {
        _entitiesToAdd.Add(entity);
    }

    public void AddEntities(IEnumerable<IEntity> entities) {
        _entitiesToAdd.AddRange(entities);
    }

    public void RemoveEntity(IEntity entity) {
        _entitiesToRemove.Add((entity, true));
    }

    public void RemoveEntities(IEnumerable<IEntity> entities) {
        _entitiesToRemove.AddRange(entities.Select(x => (x, true)));
    }

    public void MoveEntity(IEntity entity, IScene? other = null) {
        _entitiesToRemove.Add((entity, false));
        other?.AddEntity(entity);
    }

    public void MoveEntities(IEnumerable<IEntity> entities, IScene? other = null) {
        _entitiesToRemove.AddRange(entities.Select(x => (x, false)));
        other?.AddEntities(entities);
    }

    private void HandleAddEntity(IEntity e) {
        _entities.Add(e);
        e.OnAdd(this);
        OnAddEntity?.Invoke(e);
        foreach (var (type, ebt) in _entitiesByType) {
            if (e.GetType().IsAssignableTo(type)) {
                if (type.IsAssignableTo(typeof(IPriorizableEntity))) {
                    var i = ebt.BinarySearch((IPriorizableEntity)e);
                    i = i < 0 ? ~i : i;
                    ebt.List.Insert(i, e);
                } else {
                    ebt.List.Add(e);
                }
            }
        }
        if (e is IEntityHolder h) {
            foreach (var he in h.Entities) {
                HandleAddEntity(he);
            }
        }
    }

    private void HandleRemoveEntity(IEntity e, bool shouldDispose) {
        if (!_entities.Remove(e)) {
            return;
        }

        e.OnRemove(this);
        OnRemoveEntity?.Invoke(e);
        foreach (var (type, ebt) in _entitiesByType) {
            if (e.GetType().IsAssignableTo(type)) {
                ebt.List.Remove(e);
            }
        }
        if (e is IEntityHolder h) {
            foreach (var he in h.Entities) {
                HandleRemoveEntity(he, shouldDispose);
            }
        }
        if (shouldDispose && e is IDisposable d) { d.Dispose(); }
    }

    protected void DealWithEntityBuffers() {
        foreach (var e in _entitiesToAdd) {
            HandleAddEntity(e);
        }
        _entitiesToAdd.Clear();

        foreach (var (e, shouldDispose) in _entitiesToRemove) {
            HandleRemoveEntity(e, shouldDispose);
        }
        _entitiesToRemove.Clear();
    }

    public IReadOnlyList<T> GetEntitiesOfType<T>() where T : IEntity {
        if (_entitiesByType.TryGetValue(typeof(T), out var val)) {
            return (IReadOnlyList<T>)val.List;
        }

        var ofType = _entities.OfType<T>();
        if (typeof(T).IsAssignableTo(typeof(IPriorizableEntity))) {
            ofType = ofType.OrderBy(e => ((IPriorizableEntity)e).Priority);
        }

        return (IReadOnlyList<T>)(_entitiesByType[typeof(T)] = new ListWrapper<T>(ofType.ToList())).List;
    }

    // TODO: Consider: Events instead of virtual?
    public virtual void Update(float delta) {
        // TODO: We do this here because the input handling comes exactly before, not sure if that's right.
        DealWithEntityBuffers();

        // TODO: There appears to be a race condition here??
        foreach (var u in GetEntitiesOfType<IUpdatable>()) {
            u.Update(delta);
        }

        DealWithEntityBuffers();
    }

    public virtual void Tick() {
        foreach (var t in GetEntitiesOfType<ITickable>()) {
            t.Tick();
        }

        DealWithEntityBuffers();
    }

    public virtual void Prerender(float interp) {
        Parallel.ForEach(GetEntitiesOfType<IPrerenderable>(), p => p.Prerender(interp));
    }

    public virtual void Render(IRenderTarget target, float interp) {
        foreach (var r in GetEntitiesOfType<IRenderable>()) {
            r.Render(target, interp);
        }

        // Don't deal with buffers here, rendering should NOT affect them.
    }

    public virtual void OnEnter() { }
    public virtual void OnLeave() { }

    protected override void DisposeImpl() {
        // Remove in reverse order of addition, so that Dispose is called in the correct order.
        for (var i = _entities.Count - 1; i >= 0; i--) {
            RemoveEntity(_entities[i]);
        }
        DealWithEntityBuffers();
    }
}
