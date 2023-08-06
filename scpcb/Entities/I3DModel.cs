using System.Numerics;
using scpcb.Graphics.Primitives;

namespace scpcb.Entities;

public interface I3DModel {
    /// <summary>
    /// This only needs to roughly be the position of the model, as it's solely used for the rendering order of transparent objects.
    /// </summary>
    Vector3 Position { get; }
    ICBModel Model { get; }
}
