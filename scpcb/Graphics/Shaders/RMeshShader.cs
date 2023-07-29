using System.Numerics;
using scpcb.Graphics.Shaders.ConstantMembers;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;

namespace scpcb.Graphics.Shaders;

[ShaderClass]
public class RMeshShader {
    public record struct Vertex([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 Uv);
    public record struct Fragment([SystemPositionSemantic] Vector4 Position, [TextureCoordinateSemantic] Vector2 Uv);

    public struct VertUniforms : ICommonMatricesConstantMembers {
        public Matrix4x4 WorldMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
        public Matrix4x4 ProjectionMatrix { get; set; }
    }
    public VertUniforms VConstants;

    [ResourceSet(1)]
    public Texture2DResource SurfaceTexture;
    [ResourceSet(1)]
    public SamplerResource Sampler;

    [VertexShader]
    public Fragment VS(Vertex vert) {
        Fragment frag = default;
        frag.Position = Mul(VConstants.ProjectionMatrix, Mul(VConstants.ViewMatrix, Mul(VConstants.WorldMatrix, new Vector4(vert.Position, 1))));
        frag.Uv = vert.Uv;
        return frag;
    }

    [FragmentShader]
    public Vector4 FS(Fragment frag) {
        return Sample(SurfaceTexture, Sampler, frag.Uv);
    }
}

public class RMeshShaderGenerated : GeneratedShader<RMeshShader, RMeshShader.Vertex, RMeshShader.VertUniforms, Empty>,
        ISimpleShader<RMeshShaderGenerated> {
    public RMeshShaderGenerated(GraphicsResources gfxRes) : base(gfxRes) { }

    public static RMeshShaderGenerated Create(GraphicsResources gfxRes) => new(gfxRes);
}
