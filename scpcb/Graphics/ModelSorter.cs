using System.Numerics;
using System.Runtime.CompilerServices;
using scpcb.Entities;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Textures;
using scpcb.Scenes;

namespace scpcb.Graphics;

public class ModelSorter : EntityListener, IPrerenderable, IRenderable {
    private readonly List<I3DModel> _opaque = new();
    private readonly List<I3DModel> _transparent = new();

    private readonly Func<float, Vector3> _getPos;

    public ModelSorter(IScene scene, Func<float, Vector3> getPos) : base(scene) {
        _getPos = getPos;
    }

    public void Add(I3DModel model) {
        // We can't/don't sort it into the transparent objects because we don't want to ask for the pos here.
        (model.Model.IsOpaque ? _opaque : _transparent).Add(model);
    }

    public void AddRange(IEnumerable<I3DModel> models) {
        foreach (var model in models) {
            Add(model);
        }
    }

    public void Remove(I3DModel model) {
        _opaque.Remove(model);
        _transparent.Remove(model);
    }

    public void RemoveRange(IEnumerable<I3DModel> models) {
        foreach (var m in models) {
            Remove(m);
        }
    }

    protected override void OnAddEntity(IEntity e) {
        switch (e) {
            case I3DModelHolder p:
                AddRange(p.Models);
                break;
            case I3DModel m:
                Add(m);
                break;
        }
    }

    protected override void OnRemoveEntity(IEntity e) {
        switch (e) {
            case I3DModelHolder p:
                RemoveRange(p.Models);
                break;
            case I3DModel m:
                Remove(m);
                break;
        }
    }

    public void Prerender(float interp) {
        var pos = _getPos(interp);

        for (var i = 0; i < _opaque.Count; i++) {
            if (!_opaque[i].Model.IsOpaque) {
                _transparent.Add(_opaque[i]);
                _opaque.RemoveAt(i);
                i--;
                continue;
            }

            // Bubblesort, since we call this more than abundantly often and don't need the order.
            if (i > 0 && Compare(_opaque[i - 1].Model, _opaque[i].Model) < 0) {
                (_opaque[i - 1], _opaque[i]) = (_opaque[i], _opaque[i - 1]);
            }

            [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
            static int Compare(ICBModel a, ICBModel b) {
                var ret = a.Material.Shader.GetHashCode().CompareTo(b.Material.Shader.GetHashCode());
                if (ret != 0) { return ret; }
                ret = a.Material.GetHashCode().CompareTo(b.Material.GetHashCode());
                if (ret != 0) { return ret; }
                ret = a.Mesh.GetHashCode().CompareTo(b.Mesh.GetHashCode());
                return ret;
            }
        }

        for (var i = 0; i < _transparent.Count; i++) {
            if (_transparent[i].Model.IsOpaque) {
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
            target.Render(o.Model, interp);
        }

        foreach (var model in _transparent) {
            target.Render(model.Model, interp);
        }
    }

    // Must be slightly higher than normal so that regular renderables render behind the transparent models.
    public int Priority => 10;
}
