using scpcb.Graphics.Primitives;
using scpcb.Graphics.Utility;

namespace scpcb.Graphics.Shaders;

/// <summary>
/// A shader class that can simply be instantiated only using a <see cref="GraphicsResources"/> instance.
/// </summary>
/// <remarks>This allows classes to be instantiated by the <see cref="ShaderCache"/> mechanism.</remarks>
public interface ISimpleShader<T> where T : ICBShader {
    static abstract T Create(GraphicsResources gfxRes);
}
