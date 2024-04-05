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

    void AddEntity(IEntity entity);
    void AddEntities(IEnumerable<IEntity> entities) {
        foreach (var e in entities) {
            AddEntity(e);
        }
    }

    /// <summary>
    /// Removes and disposes.
    /// </summary>
    void RemoveEntity(IEntity entity);
    /// <see cref="RemoveEntity"/>
    void RemoveEntities(IEnumerable<IEntity> entities) {
        foreach (var e in entities) {
            RemoveEntity(e);
        }
    }

    /// <summary>
    /// Removes without disposing, moving it to the other scene if provided.
    /// </summary>
    void MoveEntity(IEntity entity, IScene? other = null);

    /// <see cref="MoveEntity"/>
    /// <remarks><paramref name="entities"/> is enumerated twice.</remarks>
    void MoveEntities(IEnumerable<IEntity> entities, IScene? other) {
        foreach (var e in entities) {
            MoveEntity(e, other);
        }
    }

    event Action<IEntity> OnAddEntity;
    event Action<IEntity> OnRemoveEntity;

    void OnEnter() { }
    void OnLeave() { }
}
