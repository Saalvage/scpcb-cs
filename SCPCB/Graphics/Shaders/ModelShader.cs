using ShaderGen;
using System.Numerics;
using Assimp;
using SCPCB.Graphics.Assimp;
using static ShaderGen.ShaderBuiltins;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Fragments;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class ModelShader : IAssimpMaterialConvertible<VPositionTexture, GraphicsResources>,
        IAutoShader<ModelShader.VertexConstants, ModelShader.FragmentConstants, ModelShader.InstanceVertexConstants, Empty> {

    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
    }

    public struct InstanceVertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    public struct FragmentConstants : IAmbientLightConstantMember {
        public float AmbientLightLevel { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture { get; }
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler { get; }

    [VertexShader]
    public FPositionTexture VS(VPositionTexture input) {
        FPositionTexture output;
        Vector4 worldPosition = Mul(InstanceVertexBlock.WorldMatrix, new Vector4(input.Position, 1));
        Vector4 viewPosition = Mul(VertexBlock.ViewMatrix, worldPosition);
        output.Position = Mul(VertexBlock.ProjectionMatrix, viewPosition);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        var color = Sample(SurfaceTexture, Sampler, input.TextureCoord);
        return new(color.XYZ() * new Vector3(FragmentBlock.AmbientLightLevel), color.W);
    }

    public static ICBMaterial<VPositionTexture> ConvertMaterial(Material mat, string fileDir, GraphicsResources gfxRes)
        => gfxRes.MaterialCache.GetMaterial<ModelShader, VPositionTexture>(
            [gfxRes.TextureCache.GetTexture(fileDir + '/' + Path.GetFileName(mat.TextureDiffuse.FilePath))],
            [gfxRes.GraphicsDevice.Aniso4xSampler]);
}
