using ShaderGen;
using System.Numerics;
using static ShaderGen.ShaderBuiltins;

[assembly: ShaderSet("MinExample", "MinExample.VertexShaderFunc", "MinExample.FragmentShaderFunc")]
public class MinExample {
    public Matrix4x4 Projection;
    public Matrix4x4 View;
    public Matrix4x4 World;
    public Texture2DResource SurfaceTexture;
    public SamplerResource Sampler;

    public struct VertexInput {
        [PositionSemantic] public Vector3 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
    }

    public struct FragmentInput {
        [SystemPositionSemanticAttribute] public Vector4 Position;
        [TextureCoordinateSemantic] public Vector2 TextureCoord;
    }

    [VertexShader]
    public FragmentInput VertexShaderFunc(VertexInput input) {
        FragmentInput output;
        Vector4 worldPosition = Mul(World, new Vector4(input.Position, 1));
        Vector4 viewPosition = Mul(View, worldPosition);
        output.Position = Mul(Projection, viewPosition);
        output.TextureCoord = input.TextureCoord;
        return output;
    }

    [FragmentShader]
    public Vector4 FragmentShaderFunc(FragmentInput input) {
        return Sample(SurfaceTexture, Sampler, input.TextureCoord);
    }
}
