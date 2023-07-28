using ShaderGen;
using System.Numerics;
using scpcb.Graphics.Shaders.ConstantMembers;
using static ShaderGen.ShaderBuiltins;

namespace scpcb.Graphics.Shaders;

[ShaderClass]
public class ModelShader {
    public struct VertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 Projection { get; set; }
        public Matrix4x4 View { get; set; }
        public Matrix4x4 WorldMatrix { get; set; }
    }

    public VertexConstants VConstants;

    [ResourceSet(1)]
    public Texture2DResource SurfaceTexture;
    [ResourceSet(1)]
    public SamplerResource Sampler;

    public record struct Vertex([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 TextureCoord);

    public struct FragmentInput {
        [SystemPositionSemantic] public Vector4 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
    }

    [VertexShader]
    public FragmentInput VS(Vertex input) {
        FragmentInput output;
        Vector4 worldPosition = Mul(VConstants.WorldMatrix, new Vector4(input.Position, 1));
        Vector4 viewPosition = Mul(VConstants.View, worldPosition);
        output.Position = Mul(VConstants.Projection, viewPosition);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FragmentInput input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord);
    }
}

// TODO: Look into whether merging these makes sense.
// ShaderGen would need to be modified to be able to see the VS and FS constants in the base class,
// it'd allow for removing quite some boilerplate!
// Sadly, we can't do template crazyness to get the correct amount of textures in the base class,
// so we'd need a way to know their name.
public class ModelShaderGenerated : GeneratedShader<ModelShader, ModelShader.Vertex, ModelShader.VertexConstants, Empty>,
        ISimpleShader<ModelShaderGenerated> {
    private ModelShaderGenerated(GraphicsResources gfxRes) : base(gfxRes) { }

    public static ModelShaderGenerated Create(GraphicsResources gfxRes) => new(gfxRes);
}
