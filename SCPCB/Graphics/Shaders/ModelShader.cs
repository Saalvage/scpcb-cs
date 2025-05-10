using ShaderGen;
using System.Numerics;
using Assimp;
using SCPCB.Graphics.Assimp;
using static ShaderGen.ShaderBuiltins;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class ModelShader : IAssimpMaterialConvertible<VPositionTexture, GraphicsResources>,
        IAutoShader<ModelShader.VertexConstants, ModelShader.FragmentConstants, ModelShader.InstanceVertexConstants, Empty> {

    public record struct Fragment([SystemPositionSemantic] Vector4 Position,
        [TextureCoordinateSemantic] Vector2 Uv,
        [TextureCoordinateSemantic] Vector3 CameraPos);

    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
    }

    public struct InstanceVertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    public struct FragmentConstants : IAmbientLightConstantMember, IFogRangeConstantMember {
        public Fog Fog { get; set; }
        public float AmbientLightLevel { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture { get; }
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler { get; }

    [VertexShader]
    public Fragment VS(VPositionTexture input) {
        Fragment output = default;
        Vector4 worldPosition = Mul(InstanceVertexBlock.WorldMatrix, new Vector4(input.Position, 1));
        Vector4 viewPosition = Mul(VertexBlock.ViewMatrix, worldPosition);
        output.CameraPos = viewPosition.XYZ() / viewPosition.W;
        output.Position = Mul(VertexBlock.ProjectionMatrix, viewPosition);
        output.Uv = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(Fragment input) {
        var color = Sample(SurfaceTexture, Sampler, input.Uv);
        var fogDensity = (input.CameraPos.Length() - FragmentBlock.Fog.Near) / (FragmentBlock.Fog.Far - FragmentBlock.Fog.Near);
        fogDensity = MathF.Max(0, fogDensity);
        return new(new Vector3(0, 0, 0) * fogDensity + (1 - fogDensity) * color.XYZ() * new Vector3(FragmentBlock.AmbientLightLevel), color.W);
    }

    public static ICBMaterial<VPositionTexture> ConvertMaterial(Material mat, string fileDir, GraphicsResources gfxRes)
        => gfxRes.MaterialCache.GetMaterial<ModelShader, VPositionTexture>(
            [gfxRes.TextureCache.GetTexture(fileDir + '/' + Path.GetFileName(mat.TextureDiffuse.FilePath))],
            [gfxRes.GraphicsDevice.Aniso4xSampler]);
}
