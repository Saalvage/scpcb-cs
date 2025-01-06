using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;
using SCPCB.Graphics.Primitives;
using SCPCB.Graphics.Shaders.ConstantMembers;
using SCPCB.Graphics.Shaders.Fragments;
using SCPCB.Graphics.Shaders.Utility;

#pragma warning disable CS8618

namespace SCPCB.Graphics.Shaders;

public partial class TextShader : IAutoShader<TextShader.VertexConstants, Empty, Empty, Empty> {
    public record struct VertexConstants(Matrix4x4 ProjectionMatrix, Vector3 Position) : IUIProjectionMatrixConstantMember, IPositionConstantMember;

    public record struct Vertex([PositionSemantic] Vector2 Position, [TextureCoordinateSemantic] Vector2 TexCoords);

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FPositionTexture VS(Vertex input) {
        FPositionTexture output;
        var originalPos = input.Position;
        output.Position = new(Vector3.Transform(new Vector3(originalPos, 1) + VertexBlock.Position, VertexBlock.ProjectionMatrix), 1);
        output.TextureCoord = input.TexCoords;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        return new(1, 1, 1, Sample(SurfaceTexture, Sampler, input.TextureCoord).X);
    }

    public static ShaderParameters DefaultParameters => UIShader.DefaultParameters;
}
