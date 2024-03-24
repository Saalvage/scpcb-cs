using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Utility;

#pragma warning disable CS8618

namespace scpcb.Graphics.Shaders;

public partial class UIShader : IAutoShader<UIShader.VertexConstants, Empty, Empty, Empty> {
    public record struct VertexConstants(Matrix4x4 ProjectionMatrix, Vector3 Position, float Pad, Vector2 Scale)
        : IPositionConstantMember, IUIProjectionMatrixConstantMember, IUIScaleConstantMember;

    public record struct Vertex([PositionSemantic] Vector2 Position, [TextureCoordinateSemantic] Vector2 TextureCoord);

    public struct FragmentInput {
        [SystemPositionSemantic] public Vector4 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
    }

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FragmentInput VS(Vertex input) {
        FragmentInput output;
        var originalPos = VertexBlock.Position + new Vector3(input.Position * VertexBlock.Scale, 1);
        output.Position = new(Vector3.Transform(originalPos, VertexBlock.ProjectionMatrix), 1);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FragmentInput input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord);
    }
}
