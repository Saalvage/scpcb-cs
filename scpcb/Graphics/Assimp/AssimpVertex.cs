using System.Numerics;

namespace scpcb.Graphics.Assimp;

public ref struct AssimpVertex {
    public Vector3 Position;
    public Span<Vector3> TexCoords;
    public Span<Vector4> VertexColors;
    public Vector3 Normal;
    public Vector3 Tangent;
    public Vector3 Bitangent;
}
