using System.Numerics;
using scpcb.Entities;

namespace scpcb.Graphics; 

public class ModelSorter {
    private readonly List<I3DModel> _opaque = new();
    private readonly List<I3DModel> _transparent = new();

    public void Add(I3DModel model) {
        // We can't/don't sort it into the transparent objects because we don't want to ask for the pos here.
        (model.Model.IsOpaque ? _opaque : _transparent).Add(model);
    }

    public void AddRange(IEnumerable<I3DModel> models) {
        foreach (var model in models) {
            Add(model);
        }
    }

    public void Render(RenderTarget target, Vector3 pos, float interp) {
        // TODO: It might make sense to group these by shader to avoid pipeline switches.
        // One approach would be having a list per shader and having an event on the model for a shader change.
        // Does the CPU cost warrant the GPU savings? Benchmark!
        for (var i = 0; i < _opaque.Count; i++) {
            if (_opaque[i].Model.IsOpaque) {
                target.Render(_opaque[i].Model, interp);
            } else {
                _transparent.Add(_opaque[i]);
                _opaque.RemoveAt(i);
                i--;
            }
        }

        for (var i = 0; i < _transparent.Count; i++) {
            if (_transparent[i].Model.IsOpaque) {
                // Stragglers.
                target.Render(_transparent[i].Model, interp);
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

        foreach (var model in _transparent) {
            target.Render(model.Model, interp);
        }
    }
}
