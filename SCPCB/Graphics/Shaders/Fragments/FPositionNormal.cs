using ShaderGen;
using System.Numerics;

namespace SCPCB.Graphics.Shaders.Fragments;

public struct FPositionNormal {
    [SystemPositionSemantic] public Vector4 Position;
    [NormalSemantic] public Vector3 Normal;
}
