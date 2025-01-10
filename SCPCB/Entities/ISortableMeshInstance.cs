using SCPCB.Graphics;
using System.Numerics;

namespace SCPCB.Entities;

/// <summary>
/// This only needs the rough position of the model, as it's solely used for the rendering order of transparent objects here.
/// </summary>
public interface ISortableMeshInstance : IPositionalEntity {
    IMeshInstance MeshInstance { get; }
    bool IsOpaque { get; }
}

public record SortableMeshInstance(IMeshInstance MeshInstance, Vector3 Position, bool IsOpaque) : ISortableMeshInstance;
