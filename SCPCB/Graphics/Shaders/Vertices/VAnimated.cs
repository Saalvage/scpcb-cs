using System.Numerics;
using ShaderGen;

namespace SCPCB.Graphics.Shaders.Vertices;

public interface IAnimatedVertex {
    Int4 BoneIDs { get; set; }
    Vector4 BoneWeights { get; set; }
}
