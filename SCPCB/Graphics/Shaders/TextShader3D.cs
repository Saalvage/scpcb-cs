using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Fragments;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class TextShader3D : IAutoShader<TextShader3D.VertexConstants, Empty, TextShader3D.VertexInstanceConstants, TextShader3D.FragmentInstanceConstants> {
    public record struct VertexConstants(Matrix4x4 ProjectionMatrix, Matrix4x4 ViewMatrix) : IProjectionMatrixConstantMember, IViewMatrixConstantMember;
    public record struct VertexInstanceConstants(Matrix4x4 WorldMatrix) : IWorldMatrixConstantMember;
    public record struct FragmentInstanceConstants(Vector3 Color) : IColorConstantMember;

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FPositionTexture VS(VPositionTexture2D input) {
        FPositionTexture output;
        output.Position = new(input.Position, 0, 1);
        output.Position = Vector4.Transform(output.Position, InstanceVertexBlock.WorldMatrix);
        output.Position = Vector4.Transform(output.Position, VertexBlock.ViewMatrix);
        output.Position = Vector4.Transform(output.Position, VertexBlock.ProjectionMatrix);
        output.TextureCoord = input.TexCoords;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        return new(InstanceFragmentBlock.Color, Sample(SurfaceTexture, Sampler, input.TextureCoord).X);
    }

    public static ShaderParameters DefaultParameters => ShaderParameters.Default with {
        DepthState = ShaderParameters.Default.DepthState with { DepthWriteEnabled = false },
    };
}
