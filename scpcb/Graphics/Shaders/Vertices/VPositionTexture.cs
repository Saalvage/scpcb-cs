using System.Numerics;
using SCPCB.Graphics.Assimp;
using ShaderGen;

namespace SCPCB.Graphics.Shaders.Vertices;

public record struct VPositionTexture([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 TextureCoord)
        : IAssimpVertexConvertible<VPositionTexture> {
    public static VPositionTexture ConvertVertex(AssimpVertex vert) => new(vert.Position, vert.TexCoords[0].XY());
}
