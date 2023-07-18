using Veldrid;

namespace scpcb.Graphics; 

public class ShaderCache : Disposable {
    private readonly Dictionary<Type, ICBShader> _shaders = new();

    private readonly GraphicsDevice _gfx;

    public ShaderCache(GraphicsDevice gfx) {
        _gfx = gfx;
    }

    /// <summary>
    /// Easy access to unparameterized shaders.
    /// </summary>
    /// <typeparam name="TShader"></typeparam>
    /// <returns></returns>
    public TShader GetShader<TShader>() where TShader : ICBShader {
        var type = typeof(TShader);

        if (_shaders.TryGetValue(type, out var shader)) {
            return (TShader)shader;
        }

        var ctor = type.GetConstructor(new[] { typeof(GraphicsDevice) });
        if (ctor is null) {
            throw new ArgumentException($"{type.FullName} does not feature a constructor only taking a GraphicsDevice");
        }

        var newShader = (TShader)ctor.Invoke(new object?[] { _gfx });
        _shaders.Add(typeof(TShader), newShader);
        return newShader;
    }

    protected override void DisposeImpl() {
        foreach (var (_, shader) in _shaders) {
            if (shader is IDisposable disposable) {
                disposable.Dispose();
            }
        }
    }
}
