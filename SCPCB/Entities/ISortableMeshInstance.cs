using SCPCB.Graphics;
using System.Numerics;
using SCPCB.Utility;

namespace SCPCB.Entities;

/// <summary>
/// This only needs the rough position of the model, as it's solely used for the rendering order of transparent objects here.
/// </summary>
public interface ISortableMeshInstance : IEntity, IPositioned {
    IMeshInstance MeshInstance { get; }
    bool IsOpaque { get; }
}

public record SortableMeshInstance(IMeshInstance MeshInstance, Vector3 Position, bool IsOpaque) : ISortableMeshInstance;

public record DependentSortableMeshInstance(IMeshInstance MeshInstance, ITransformable Dependent, bool IsOpaque) : ISortableMeshInstance {
    public Vector3 Position => Dependent.WorldTransform.Position;
}
