using System.Numerics;
using Veldrid;

namespace scpcb.Graphics.Shaders; 

public class RMeshShader : CBShader<RMeshShader.Vertex, RMeshShader.VertUniforms, RMeshShader.FragUniforms> {
    public record struct Vertex(Vector3 Position, Vector2 Uv, Vector2 Uv2, Vector3 Color);

    public record struct VertUniforms;
    public record struct FragUniforms;

    public RMeshShader(GraphicsDevice gfx) : base(gfx, "", "", 0) {

    }
}
