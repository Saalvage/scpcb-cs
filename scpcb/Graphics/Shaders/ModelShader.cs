using ShaderGen;
using System.Numerics;
using Assimp;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.Shaders.ConstantMembers;
using static ShaderGen.ShaderBuiltins;
using scpcb.Graphics.Primitives;
#pragma warning disable CS8618

namespace scpcb.Graphics.Shaders;

[ShaderClass]
public class ModelShader : IAssimpMaterialConvertible<ModelShader.Vertex, ICBMaterial<ModelShader.Vertex>> {
    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
    }

    public VertexConstants VConstants;

    public struct InstanceVertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    [ResourceSet(1)]
    public InstanceVertexConstants InstVConstants;

    [ResourceSet(2)]
    public Texture2DResource SurfaceTexture;
    [ResourceSet(2)]
    public SamplerResource Sampler;

    public record struct Vertex([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 TextureCoord)
            : IAssimpVertexConvertible<Vertex> {
        public static Vertex ConvertVertex(AssimpVertex vert) => new(vert.Position, vert.TexCoords[0].XY());
    }

    public struct FragmentInput {
        [SystemPositionSemantic] public Vector4 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
    }

    [VertexShader]
    public FragmentInput VS(Vertex input) {
        FragmentInput output;
        Vector4 worldPosition = Mul(InstVConstants.WorldMatrix, new Vector4(input.Position, 1));
        Vector4 viewPosition = Mul(VConstants.ViewMatrix, worldPosition);
        output.Position = Mul(VConstants.ProjectionMatrix, viewPosition);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FragmentInput input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord);
    }

    public static ICBMaterial<Vertex> ConvertMaterial(Material mat, ICBMaterial<Vertex> plugin) => plugin;
}

// TODO: Look into whether merging these makes sense.
// ShaderGen would need to be modified to be able to see the VS and FS constants in the base class,
// it'd allow for removing quite some boilerplate!
// Sadly, we can't do template crazyness to get the correct amount of textures in the base class,
// so we'd need a way to know their name.
// 
// Another approach is to parameterize GeneratedShader via reflection, however, due to C#'s weak
// type inference this would clutter the call sites and prevent direct access to the shader constants.
public class ModelShaderGenerated : GeneratedShader<ModelShader, ModelShader.Vertex, ModelShader.VertexConstants,
        Empty, ModelShader.InstanceVertexConstants, Empty>, ISimpleShader<ModelShaderGenerated> {
    private ModelShaderGenerated(GraphicsResources gfxRes) : base(gfxRes) { }

    public static ModelShaderGenerated Create(GraphicsResources gfxRes) => new(gfxRes);
}
