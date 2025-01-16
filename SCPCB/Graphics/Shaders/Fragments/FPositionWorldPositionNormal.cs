using ShaderGen;
using System.Numerics;

namespace SCPCB.Graphics.Shaders.Fragments;

public struct FPositionWorldPositionNormal {
    [SystemPositionSemantic] public Vector4 Position;
    [PositionSemantic] public Vector4 WorldPosition;
    [NormalSemantic] public Vector3 Normal;
}
