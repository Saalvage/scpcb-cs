using ShaderGen;
using System.Numerics;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.Utility;
using static ShaderGen.ShaderBuiltins;
using scpcb.Graphics.Shaders.Vertices;

#pragma warning disable CS8618

namespace scpcb.Graphics.Shaders;

public partial class SpriteShader : IAutoShader<SpriteShader.VertexConstants, Empty,
        SpriteShader.InstanceVertexConstants, SpriteShader.InstanceFragmentConstants> {

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

    public struct InstanceFragmentConstants : IColorConstantMember {
        public Vector3 Color { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FragmentInput VS(VPositionTexture input) {
        FragmentInput output;
        output.Position = Mul(VertexBlock.ProjectionMatrix,
            Mul(VertexBlock.ViewMatrix,
                Mul(InstanceVertexBlock.WorldMatrix, new(input.Position, 1))));
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FragmentInput input) {
        return new(Sample(SurfaceTexture, Sampler, input.TextureCoord).XYZ() * InstanceFragmentBlock.Color, 1f);
    }
}
