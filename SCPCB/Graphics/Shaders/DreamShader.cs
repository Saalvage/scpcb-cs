using ShaderGen;
using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using static ShaderGen.ShaderBuiltins;
using SCPCB.Graphics.Shaders.Fragments;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;
using Veldrid;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class DreamShader : IAutoShader<DreamShader.VertexConstants, Empty, DreamShader.VertexInstanceConstants, DreamShader.FragmentInstanceConstants> {
    public struct VertexConstants : IUIProjectionMatrixConstantMember {
        public Matrix4x4 ProjectionMatrix { get; set; }
    }

    public struct VertexInstanceConstants : IPositionConstantMember {
        public Vector3 Position { get; set; }
    }

    public struct FragmentInstanceConstants : IBlurStrengthConstantMember {
        public float BlurStrength { get; set; }
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FPositionTexture VS(VPositionTexture input) {
        FPositionTexture output;
        output.Position = Mul(VertexBlock.ProjectionMatrix, new(input.Position + InstanceVertexBlock.Position, 1));
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        // TODO: We're making it "darker" here by multiplying all channels with the blur factor, not sure if that's desired.
        return Sample(SurfaceTexture, Sampler, input.TextureCoord) * InstanceFragmentBlock.BlurStrength;
    }

    public static ShaderParameters DefaultParameters { get; } = ShaderParameters.Default with {
        DepthState = DepthStencilStateDescription.Disabled,
        BlendState = BlendStateDescription.SingleAlphaBlend,
    };
}
