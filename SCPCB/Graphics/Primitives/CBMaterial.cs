using Veldrid;

namespace SCPCB.Graphics.Primitives;

public interface ICBMaterial : IDisposable {
    ICBShader Shader { get; }
    IReadOnlyList<ICBTexture> Textures { get; }

    void ApplyTextures(CommandList commands);
}

public interface ICBMaterial<TVertex> : ICBMaterial {
    new ICBShader<TVertex> Shader { get; }
    ICBShader ICBMaterial.Shader => Shader;
}
