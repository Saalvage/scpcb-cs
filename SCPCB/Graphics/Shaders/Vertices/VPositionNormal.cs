using System.Numerics;
using SCPCB.Graphics.Assimp;
using ShaderGen;

namespace SCPCB.Graphics.Shaders.Vertices;

public record struct VPositionNormal([PositionSemantic] Vector3 Position, [NormalSemantic] Vector3 Normal)
        : IAssimpVertexConvertible<VPositionNormal> {
    public static VPositionNormal ConvertVertex(AssimpVertex vert) => new(vert.Position, vert.Normal);
}
