using SCPCB.Entities;
using SCPCB.Utility;

namespace SCPCB.Serialization;

/// <summary>
/// Allows for restoring relationships between entities by storing the hash code of the ENTITY (not the data) in the data.
/// </summary>
public interface IReferenceResolver {
    void Resolve(long hashCode, Action<IEntity> onResolve);
}

public interface IReferenceResolverImpl : IReferenceResolver {
    void SubmitEntity(long hashCode, IEntity entity);
}

public class ReferenceResolver : Disposable, IReferenceResolverImpl {
    private readonly Dictionary<long, List<Action<IEntity>>> _hashReferences = [];

    private readonly Dictionary<long, IEntity> _entities = [];

    public void Resolve(long id, Action<IEntity> onResolve) {
        if (_entities.TryGetValue(id, out var entity)) {
            onResolve(entity);
        } else {
            _hashReferences[id] = _hashReferences.TryGetValue(id, out var list) ? list : [];
            _hashReferences[id].Add(onResolve);
        }
    }

    public void SubmitEntity(long hashCode, IEntity entity) {
        _entities.Add(hashCode, entity);
        if (_hashReferences.TryGetValue(hashCode, out var list)) {
            foreach (var action in list) {
                action(entity);
            }

            _hashReferences.Remove(hashCode);
        }
    }

    public void AssertAllReferencesResolved() {
        if (_hashReferences.Count != 0) {
            throw new();
        }
    }

    protected override void DisposeImpl() {
        AssertAllReferencesResolved();
    }
}
