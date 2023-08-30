using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;
using scpcb.Graphics.Primitives;

#pragma warning disable CS8618

namespace scpcb.Graphics.Shaders;

[ShaderClass]
public class UIShader : IAutoShader<Empty, Empty, Empty, Empty> {
    public Texture2DResource SurfaceTexture;
    public SamplerResource Sampler;

    public record struct Vertex([PositionSemantic] Vector2 Position, [TextureCoordinateSemantic] Vector2 TextureCoord);

    public struct FragmentInput {
        [SystemPositionSemantic] public Vector4 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
    }

    [VertexShader]
    public FragmentInput VS(Vertex input) {
        FragmentInput output;
        output.Position = new(input.Position, 1, 1);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FS(FragmentInput input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord);
    }
}
