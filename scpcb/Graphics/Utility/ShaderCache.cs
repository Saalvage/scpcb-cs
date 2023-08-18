using System.Diagnostics;
using System.Reflection;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders;
using scpcb.Utility;
using ShaderGen;

namespace scpcb.Graphics.Utility;

// TODO: Add a way to get non-generated shaders.
public class ShaderCache : Disposable {
    private readonly WeakDictionary<Type, ICBShader> _shaders = new();

    private readonly GraphicsResources _gfxRes;

    public ShaderCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public ICBShader GetShader<TShader>() where TShader : IAutoShader {
        var type = typeof(TShader);

        if (_shaders.TryGetValue(type, out var shader)) {
            return shader;
        }

        var newShader = CreateGeneratedShader<TShader>(GetVertexTypeFromVS<TShader>());
        _shaders.Add(type, newShader);
        return newShader;
    }

    public ICBShader<TVertex> GetShader<TShader, TVertex>() where TShader : IAutoShader {
        var type = typeof(TShader);

        if (_shaders.TryGetValue(type, out var shader)) {
            return (ICBShader<TVertex>)shader;
        }

        var newShader = (ICBShader<TVertex>)CreateGeneratedShader<TShader>(typeof(TVertex));
        _shaders.Add(type, newShader);
        return newShader;
    }

    private ICBShader CreateGeneratedShader<TShader>(Type vertexType) where TShader : IAutoShader {
        Debug.Assert(GetVertexTypeFromVS<TShader>() == vertexType,
            "Tried getting shader from cache with incorret vertex type.");

        var parameters = typeof(TShader).GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAutoShader<,,,>))
            .GetGenericArguments();

        var parameterizedType = typeof(GeneratedShader<,,,,,>)
            .MakeGenericType(typeof(TShader),
                vertexType,
                parameters[0],
                parameters[1],
                parameters[2],
                parameters[3]
            );
        var ctor = parameterizedType.GetConstructor(new[] { typeof(GraphicsResources) });
        Debug.Assert(ctor != null, "Could not find required ctor on GeneratedShader!?");
        return (ICBShader)ctor.Invoke(new object?[] { _gfxRes });
    }

    private Type GetVertexTypeFromVS<TShader>()
        => typeof(TShader).GetMethods()
            .Single(x => x.GetCustomAttribute<VertexShaderAttribute>() != null)
            .GetParameters()
            .Single().ParameterType;

    public IEnumerable<ICBShader> ActiveShaders => _shaders.Select(x => x.Value);

    protected override void DisposeImpl() {
        foreach (var (_, shaderWeak) in _shaders) {
            shaderWeak.Dispose();
        }
    }
}
