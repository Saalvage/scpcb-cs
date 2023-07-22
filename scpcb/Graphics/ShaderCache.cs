using scpcb.Graphics.Shaders;

namespace scpcb.Graphics;

public class ShaderCache : Disposable {
    private readonly Dictionary<Type, ICBShader> _shaders = new();

    private readonly GraphicsResources _gfxRes;

    public ShaderCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    /// <summary>
    /// Easy access to unparameterized shaders.
    /// </summary>
    /// <typeparam name="TShader"></typeparam>
    /// <returns></returns>
    public TShader GetShader<TShader>() where TShader : ICBShader, ISimpleShader<TShader> {
        var type = typeof(TShader);

        if (_shaders.TryGetValue(type, out var shader)) {
            return (TShader)shader;
        }

        var newShader = TShader.Create(_gfxRes);
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
