using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.ConstantMembers;
using scpcb.Graphics.Shaders.Utility;
using scpcb.Graphics.Shaders.Fragments;

#pragma warning disable CS8618

namespace scpcb.Graphics.Shaders;

public partial class UIShader : IAutoShader<UIShader.VertexConstants, UIShader.FragmentConstants, Empty, Empty> {
    public record struct VertexConstants(Matrix4x4 ProjectionMatrix, Vector4 TexCoords, Vector3 Position, float Pad, Vector2 Scale)
        : IPositionConstantMember, IUIProjectionMatrixConstantMember, IUIScaleConstantMember, ITexCoordsConstantMember;

    public record struct FragmentConstants(Vector3 Color) : IColorConstantMember;

    public record struct Vertex([PositionSemantic] Vector2 Position);

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FPositionTexture VS(Vertex input) {
        FPositionTexture output;
        var originalPos = VertexBlock.Position + new Vector3(input.Position * VertexBlock.Scale, 1);
        output.Position = new(Vector3.Transform(originalPos, VertexBlock.ProjectionMatrix), 1);
        output.TextureCoord = new(VertexBlock.TexCoords[Mod((int)VertexID, 2)],
            VertexBlock.TexCoords[2 + (int)VertexID / 2]);
        return output;
    }

    private static int Mod(int a, int b) {
        return a - (b * (a / b));
    }

    [FragmentShader]
    public Vector4 FS(FPositionTexture input) {
        var sampled = Sample(SurfaceTexture, Sampler, input.TextureCoord);
        return new(FragmentBlock.Color * sampled.XYZ(), sampled.W);
    }
}
