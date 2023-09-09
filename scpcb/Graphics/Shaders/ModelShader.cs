using ShaderGen;
using System.Numerics;
using Assimp;
using scpcb.Graphics.Assimp;
using scpcb.Graphics.Shaders.ConstantMembers;
using static ShaderGen.ShaderBuiltins;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.Utility;
using scpcb.Graphics.Shaders.Vertices;
using scpcb.Utility;

#pragma warning disable CS8618

namespace scpcb.Graphics.Shaders;

public partial class ModelShader : IAssimpMaterialConvertible<VPositionTexture, GraphicsResources>,
        IAutoShader<ModelShader.VertexConstants, Empty, ModelShader.InstanceVertexConstants, Empty> {

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
    public FragmentInput VS(VPositionTexture input) {
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

    public static ICBMaterial<VPositionTexture> ConvertMaterial(Material mat, string fileDir, GraphicsResources gfxRes)
        => gfxRes.MaterialCache.GetMaterial<ModelShader, VPositionTexture>(
            gfxRes.TextureCache.GetTexture(fileDir + '/' + Path.GetFileName(mat.TextureDiffuse.FilePath)).AsEnumerableElement(),
            gfxRes.GraphicsDevice.Aniso4xSampler.AsEnumerableElement());
}
