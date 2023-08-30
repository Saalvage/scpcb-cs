using System.Numerics;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Utility;
using ShaderGen;
using Veldrid;
using static ShaderGen.ShaderBuiltins;

namespace scpcb.Graphics.Shaders;

[ShaderClass]
public class LineShader : IAutoShader<LineShader.VertexConstants, Empty,
        LineShader.InstanceVertexConstants, LineShader.InstanceFragmentConstants> {

    // TODO: This breaks when turned into a record struct (without args).
    public struct Vertex {
        [PositionSemantic] public Vector3 Position;
    }

    public struct FragmentInput {
        [SystemPositionSemantic] public Vector4 Position;
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

    [ResourceSet(0)] public VertexConstants VertexBlock { get; }
    [ResourceIgnore] public Empty FragmentBlock { get; }

    [ResourceSet(1)] public InstanceVertexConstants InstanceVertexBlock { get; }
    [ResourceSet(1)] public InstanceFragmentConstants InstanceFragmentBlock { get; }

    [VertexShader]
    public FragmentInput VS(Vertex input) {
        FragmentInput output;
        output.Position = Mul(InstanceVertexBlock.WorldMatrix, new(input.Position, 1));
        output.Position = Mul(VertexBlock.ViewMatrix, output.Position);
        output.Position = Mul(VertexBlock.ProjectionMatrix, output.Position);
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FragmentInput frag) {
        return new(InstanceFragmentBlock.Color, 1);
    }

    public static ShaderParameters DefaultParameters { get; } = new(BlendStateDescription.SingleAlphaBlend,
        DepthStencilStateDescription.DepthOnlyLessEqual, RasterizerStateDescription.CullNone,
        PrimitiveTopology.LineStrip);
}
