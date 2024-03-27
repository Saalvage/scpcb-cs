using ShaderGen;
using System.Numerics;

namespace scpcb.Graphics.Shaders.Fragments;

public struct FPositionTexture {
    [SystemPositionSemantic] public Vector4 Position;
    [TextureCoordinateSemantic] public Vector2 TextureCoord;
}
