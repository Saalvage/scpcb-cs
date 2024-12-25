using ShaderGen;
using System.Numerics;

namespace SCPCB.Graphics.Shaders.Fragments;

public struct FPositionTexture {
    [SystemPositionSemantic] public Vector4 Position;
    [TextureCoordinateSemantic] public Vector2 TextureCoord;
}
