using ShaderGen;
using System.Numerics;
using scpcb.Graphics.Assimp;

namespace scpcb.Graphics.Shaders.Vertices;

public record struct VPosition([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 TextureCoord)
        : IAssimpVertexConvertible<VPosition> {
    public static VPosition ConvertVertex(AssimpVertex vert) => new(vert.Position, vert.TexCoords[0].XY());
}
