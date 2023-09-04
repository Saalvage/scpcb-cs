using ShaderGen;
using System.Numerics;
using Assimp;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.Shaders.ConstantMembers;
using static ShaderGen.ShaderBuiltins;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.Utility;
using scpcb.Utility;

#pragma warning disable CS8618

namespace scpcb.Graphics.Shaders;

using Plugin = (GraphicsResources GraphicsResources, string BaseFilePath);

public partial class ModelShader : IAssimpMaterialConvertible<ModelShader.Vertex, Plugin>,
        IAutoShader<ModelShader.VertexConstants, Empty, ModelShader.InstanceVertexConstants, Empty> {

    public record struct Vertex([PositionSemantic] Vector3 Position, [TextureCoordinateSemantic] Vector2 TextureCoord)
            : IAssimpVertexConvertible<Vertex> {
        public static Vertex ConvertVertex(AssimpVertex vert) => new(vert.Position, vert.TexCoords[0].XY());
    }

    public struct FragmentInput {
        [SystemPositionSemantic] public Vector4 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
    }

    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
    }

    public struct InstanceVertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture { get; }
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler { get; }

    [VertexShader]
    public FragmentInput VS(Vertex input) {
        FragmentInput output;
        Vector4 worldPosition = Mul(InstanceVertexBlock.WorldMatrix, new Vector4(input.Position, 1));
        Vector4 viewPosition = Mul(VertexBlock.ViewMatrix, worldPosition);
        output.Position = Mul(VertexBlock.ProjectionMatrix, viewPosition);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FragmentInput input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord);
    }

    public static ICBMaterial<Vertex> ConvertMaterial(Material mat, Plugin plugin)
        => plugin.GraphicsResources.MaterialCache.GetMaterial<ModelShader, Vertex>(
            plugin.GraphicsResources.TextureCache.GetTexture(plugin.BaseFilePath + mat.TextureDiffuse.FilePath).AsEnumerableElement(),
            plugin.GraphicsResources.GraphicsDevice.Aniso4xSampler.AsEnumerableElement());
}
