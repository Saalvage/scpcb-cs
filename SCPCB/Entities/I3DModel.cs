using SCPCB.Graphics.Primitives;

namespace SCPCB.Entities;

/// <summary>
/// This only needs the rough position of the model, as it's solely used for the rendering order of transparent objects here.
/// </summary>
public interface I3DModel : I3DEntity {
    // TODO: Should we expose the model here? Could alternatively be done via a render method.
    ICBModel Model { get; }

    bool IsOpaque { get; }
}
