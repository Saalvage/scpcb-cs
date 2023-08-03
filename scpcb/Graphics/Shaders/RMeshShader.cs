using System.Drawing;
using System.Numerics;
using scpcb.Graphics.Shaders.ConstantMembers;
using ShaderGen;
using static ShaderGen.ShaderBuiltins;

namespace scpcb.Graphics.Shaders;

[ShaderClass]
public class RMeshShader {
    public record struct Vertex([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 Uv, [TextureCoordinateSemantic] Vector2 LmUv, [ColorSemantic] Vector3 Color);
    public record struct Fragment([SystemPositionSemantic] Vector4 Position, [TextureCoordinateSemantic] Vector2 Uv, [TextureCoordinateSemantic] Vector2 LmUv, [ColorSemantic] Vector3 Color);

    public struct VertUniforms : ICommonMatricesConstantMembers {
        public Matrix4x4 WorldMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
        public Matrix4x4 ProjectionMatrix { get; set; }
    }
    public VertUniforms VConstants;

    [ResourceSet(1)]
    public Texture2DResource LightmapTexture;
    [ResourceSet(1)]
    public Texture2DResource SurfaceTexture;
    [ResourceSet(1)]
    public SamplerResource Sampler;

    [VertexShader]
    public Fragment VS(Vertex vert) {
        Fragment frag = default;
        frag.Position = Mul(VConstants.ProjectionMatrix, Mul(VConstants.ViewMatrix, Mul(VConstants.WorldMatrix, new Vector4(vert.Position, 1))));
        frag.Uv = vert.Uv;
        frag.LmUv = vert.LmUv;
        frag.Color = vert.Color;
        return frag;
    }

    [FragmentShader]
    public Vector4 FS(Fragment frag) {
        return 2 * (Sample(SurfaceTexture, Sampler, frag.Uv) * new Vector4(frag.Color, 1f)) * Sample(LightmapTexture, Sampler, frag.LmUv);
    }
}

public class RMeshShaderGenerated : GeneratedShader<RMeshShader, RMeshShader.Vertex, RMeshShader.VertUniforms, Empty>,
        ISimpleShader<RMeshShaderGenerated> {
    public RMeshShaderGenerated(GraphicsResources gfxRes) : base(gfxRes) { }

    public static RMeshShaderGenerated Create(GraphicsResources gfxRes) => new(gfxRes);
}
