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
    public record struct VertexConstants(Matrix4x4 ProjectionMatrix, Vector4 TexCoords, Vector3 Position, float Pad, Vector2 Scale, Vector2 SinCosDeg)
        : IPositionConstantMember, IUIProjectionMatrixConstantMember, IUIScaleConstantMember, ITexCoordsConstantMember, IRotation2DConstantMember;

    public record struct FragmentConstants(Vector4 Color) : IColorAlphaConstantMember;

    public record struct Vertex([PositionSemantic] Vector2 Position);

    [ResourceSet(MATERIAL_OFFSET)] public Texture2DResource SurfaceTexture;
    [ResourceSet(MATERIAL_OFFSET)] public SamplerResource Sampler;

    [VertexShader]
    public FPositionTexture VS(Vertex input) {
        FPositionTexture output;
        var basePos = input.Position * VertexBlock.Scale;
        // TODO: Matrix2x2. Take the bitter pill and just supply one Matrix3x3 for everything?
        var originalPos = VertexBlock.Position + new Vector3(
            basePos.X * VertexBlock.SinCosDeg.Y - basePos.Y * VertexBlock.SinCosDeg.X,
            basePos.Y * VertexBlock.SinCosDeg.Y + basePos.X * VertexBlock.SinCosDeg.X,
            1);
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
        var color = Sample(SurfaceTexture, Sampler, input.TextureCoord);
        // TODO: Implement Z-sorting.
        if (color.W == 0) {
            Discard();
        }
        return color * FragmentBlock.Color;
    }
}
