using ShaderGen;
using System.Numerics;

namespace scpcb.Graphics.Shaders.Fragments;

public struct FPosition {
    [SystemPositionSemantic] public Vector4 Position;
}
