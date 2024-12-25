using System.Numerics;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Fragments;
using SCPCB.Graphics.Shaders.Utility;
using SCPCB.Graphics.Shaders.Vertices;
using ShaderGen;
using Veldrid;
using static ShaderGen.ShaderBuiltins;

namespace SCPCB.Graphics.Shaders;

public partial class LineShader : IAutoShader<LineShader.VertexConstants, Empty,
        LineShader.InstanceVertexConstants, LineShader.InstanceFragmentConstants> {

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

    [VertexShader]
    public FPosition VS(VPosition input) {
        FPosition output;
        output.Position = Mul(InstanceVertexBlock.WorldMatrix, new(input.Position, 1));
        output.Position = Mul(VertexBlock.ViewMatrix, output.Position);
        output.Position = Mul(VertexBlock.ProjectionMatrix, output.Position);
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPosition frag) {
        return new(InstanceFragmentBlock.Color, 1);
    }

    public static ShaderParameters DefaultParameters { get; } = new(BlendStateDescription.SingleAlphaBlend,
        DepthStencilStateDescription.DepthOnlyLessEqual, RasterizerStateDescription.CullNone,
        PrimitiveTopology.LineStrip);
}
