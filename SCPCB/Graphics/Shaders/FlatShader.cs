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

public partial class FlatShader : IAssimpMaterialConvertible<VPositionNormal, GraphicsResources>,
        IAutoShader<FlatShader.VertexConstants, FlatShader.FragmentConstants, FlatShader.InstanceVertexConstants, Empty> {

    public struct VertexConstants : IProjectionMatrixConstantMember, IViewMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }
    }

    public struct InstanceVertexConstants : IWorldMatrixConstantMember {
        public Matrix4x4 WorldMatrix { get; set; }
    }

    public struct FragmentConstants : IViewPositionConstantMember {
        public Vector3 ViewPosition { get; set; }
    }

    [VertexShader]
    public FPositionWorldPositionNormal VS(VPositionNormal input) {
        FPositionWorldPositionNormal output;
        output.WorldPosition = Mul(InstanceVertexBlock.WorldMatrix, new(input.Position, 1));
        var viewPosition = Mul(VertexBlock.ViewMatrix, output.WorldPosition);
        output.Position = Mul(VertexBlock.ProjectionMatrix, viewPosition);
        output.Normal = Vector3.Normalize(Mul(InstanceVertexBlock.WorldMatrix, new(input.Normal, 0)).XYZ());
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionWorldPositionNormal input) {
        return new(new(Vector3.Dot(
            Vector3.Normalize(FragmentBlock.ViewPosition - input.WorldPosition.XYZ()),
            Vector3.Normalize(input.Normal))), 1);
    }

    public static ICBMaterial<VPositionNormal> ConvertMaterial(Material mat, string fileDir, GraphicsResources gfxRes)
        => gfxRes.MaterialCache.GetMaterial<FlatShader, VPositionNormal>();
}
