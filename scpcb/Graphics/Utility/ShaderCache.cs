using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Utility;

namespace scpcb.Graphics.Utility;

public class ShaderCache : Disposable {
    private readonly WeakDictionary<Type, ICBShader> _shaders = new();

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
        _shaders.Add(type, newShader);
        return newShader;
    }

    public IEnumerable<ICBShader> ActiveShaders => _shaders.Select(x => x.Value);

    protected override void DisposeImpl() {
        foreach (var (_, shaderWeak) in _shaders) {
            shaderWeak.Dispose();
        }
    }
}
