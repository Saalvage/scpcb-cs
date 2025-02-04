using ShaderGen;
using System.Numerics;

namespace SCPCB.Graphics.Shaders.Vertices;

public record struct VPositionTexture2D([PositionSemantic] Vector2 Position, [TextureCoordinateSemantic] Vector2 TexCoords);
