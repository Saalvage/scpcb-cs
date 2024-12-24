using ShaderGen;
using System.Numerics;

namespace SCPCB.Graphics.Shaders.Fragments;

public struct FPosition {
    [SystemPositionSemantic] public Vector4 Position;
}
