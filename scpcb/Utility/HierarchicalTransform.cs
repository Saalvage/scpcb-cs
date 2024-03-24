namespace scpcb.Utility; 

public class HierarchicalTransform {
    // Which one to store and which one to calculate on the fly is up to preference.
    // I'm going with a stored world, because it's used by rendering and its interpolation
    // and I suspect the local transform is much less likely to be read/written to.
    private Transform _world = new();
    public Transform World {
        get => _world;
        set => UpdateWorld(value);
    }

    public Transform Local {
        get => _parent == null ? _world : _world - _parent.World;
        set => UpdateWorld(_parent == null ? value : _parent._world + value);
    }

    private void UpdateWorld(Transform newWorld) {
        var delta = newWorld - _world;

        foreach (var c in _children) {
            c._world += delta;
        }
        _world = newWorld;
    }

    private HierarchicalTransform? _parent;
    public HierarchicalTransform? Parent {
        get => _parent;
        set {
            _parent?._children.Remove(this);
            _parent = value;
            _parent?._children.Add(this);
        }
    }

    private readonly List<HierarchicalTransform> _children = [];
    public IReadOnlyList<HierarchicalTransform> Children => _children;

    public IEnumerable<HierarchicalTransform> Descendants {
        get {
            foreach (var c in _children) {
                yield return c;
                foreach (var d in c.Descendants) {
                    yield return d;
                }
            }
        }
    }
}
