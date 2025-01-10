using System.Numerics;
using System.Runtime.CompilerServices;
using SCPCB.Entities;
using SCPCB.Graphics.Textures;
using SCPCB.Scenes;

namespace SCPCB.Graphics;

public class SortedRenderer : EntityListener, IPrerenderable, IRenderable {
    private readonly List<ISortableMeshInstance> _opaque = [];
    private readonly List<ISortableMeshInstance> _transparent = [];

    private readonly Func<float, Vector3> _getPos;

    public SortedRenderer(IScene scene, Func<float, Vector3> getPos) : base(scene) {
        _getPos = getPos; // TODO: This doesn't use interpolation.
    }

    public void Add(ISortableMeshInstance sortable) {
        // We can't/don't sort it into the transparent objects because we don't want to ask for the pos here.
        (sortable.IsOpaque ? _opaque : _transparent).Add(sortable);
    }

    public void AddRange(IEnumerable<ISortableMeshInstance> models) {
        foreach (var model in models) {
            Add(model);
        }
    }

    public void Remove(ISortableMeshInstance sortable) {
        _opaque.Remove(sortable);
        _transparent.Remove(sortable);
    }

    public void RemoveRange(IEnumerable<ISortableMeshInstance> models) {
        foreach (var m in models) {
            Remove(m);
        }
    }

    protected override void OnAddEntity(IEntity e) {
        switch (e) {
            case ISortableMeshInstanceHolder p:
                AddRange(p.Models);
                break;
            case ISortableMeshInstance m:
                Add(m);
                break;
        }
    }

    protected override void OnRemoveEntity(IEntity e) {
        switch (e) {
            case ISortableMeshInstanceHolder p:
                RemoveRange(p.Models);
                break;
            case ISortableMeshInstance m:
                Remove(m);
                break;
        }
    }

    public void Prerender(float interp) {
        var pos = _getPos(interp);

        for (var i = 0; i < _opaque.Count; i++) {
            if (!_opaque[i].IsOpaque) {
                _transparent.Add(_opaque[i]);
                _opaque.RemoveAt(i);
                i--;
                continue;
            }

            // Bubblesort, since we call this more than abundantly often and don't need the order.
            if (i > 0 && Compare(_opaque[i - 1].MeshInstance, _opaque[i].MeshInstance) < 0) {
                (_opaque[i - 1], _opaque[i]) = (_opaque[i], _opaque[i - 1]);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            static int Compare(IMeshInstance a, IMeshInstance b) {
                var ret = a.Material.Shader.GetHashCode().CompareTo(b.Material.Shader.GetHashCode());
                if (ret != 0) { return ret; }
                ret = a.Material.GetHashCode().CompareTo(b.Material.GetHashCode());
                if (ret != 0) { return ret; }
                ret = a.Mesh.GetHashCode().CompareTo(b.Mesh.GetHashCode());
                return ret;
            }
        }

        for (var i = 0; i < _transparent.Count; i++) {
            if (_transparent[i].IsOpaque) {
                _opaque.Add(_transparent[i]);
                _transparent.RemoveAt(i);
                i--;
                continue;
            }

            // Insertion sort since we expect them to be almost sorted.
            if (i > 0 && Vector3.DistanceSquared(_transparent[i - 1].Position, pos)
                       < Vector3.DistanceSquared(_transparent[i].Position, pos)) {

                (_transparent[i - 1], _transparent[i]) = (_transparent[i], _transparent[i - 1]);
                for (var j = i - 1; j > 0; j--) {
                    if (Vector3.DistanceSquared(_transparent[j - 1].Position, pos)
                     >= Vector3.DistanceSquared(_transparent[j].Position, pos)) {
                        break;
                    }

                    (_transparent[j - 1], _transparent[j]) = (_transparent[j], _transparent[j - 1]);
                }
            }
        }
    }

    public void Render(IRenderTarget target, float interp) {
        foreach (var o in _opaque) {
            o.MeshInstance.Render(target, interp);
        }

        foreach (var model in _transparent) {
            model.MeshInstance.Render(target, interp);
        }
    }

    // Must be higher than normal so that regular renderables render behind the transparent models.
    public int Priority => 1000;
}
