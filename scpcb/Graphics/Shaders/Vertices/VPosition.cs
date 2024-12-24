using ShaderGen;
using System.Numerics;
using SCPCB.Graphics.Assimp;

namespace SCPCB.Graphics.Shaders.Vertices;

public record struct VPosition([PositionSemantic] Vector3 Position) : IAssimpVertexConvertible<VPosition> {
    public static VPosition ConvertVertex(AssimpVertex vert) => new(vert.Position);
}
