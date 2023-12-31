﻿using System.Diagnostics;
using System.Reflection;
using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.Utility;
using ShaderGen;

namespace scpcb.Graphics.Caches;

// TODO: Add a way to get non-generated shaders.
public class ShaderCache : BaseCache<(Type, ShaderParameters?), ICBShader> {
    private readonly GraphicsResources _gfxRes;

    public ShaderCache(GraphicsResources gfxRes) {
        _gfxRes = gfxRes;
    }

    public ICBShader GetShader<TShader>(Func<ShaderParameters, ShaderParameters>? shaderParameterModifications = null) where TShader : IAutoShader
        => GetShaderInternal<TShader>(GetVertexTypeFromVS<TShader>(), shaderParameterModifications);

    public ICBShader<TVertex> GetShader<TShader, TVertex>(Func<ShaderParameters, ShaderParameters>? shaderParameterModifications = null) where TShader : IAutoShader
        => (ICBShader<TVertex>)GetShaderInternal<TShader>(typeof(TVertex), shaderParameterModifications);

    private ICBShader GetShaderInternal<TShader>(Type vertexType, Func<ShaderParameters, ShaderParameters>? shaderParameterModifications)
            where TShader : IAutoShader {
        var type = typeof(TShader);

        var usedParams = shaderParameterModifications?.Invoke(TShader.DefaultParameters) ?? TShader.DefaultParameters;

        if (_dic.TryGetValue((type, usedParams), out var shader)) {
            return shader;
        }

        var newShader = CreateGeneratedShader<TShader>(vertexType, usedParams);
        _dic.Add((type, usedParams), newShader);
        return newShader;
    }

    private static readonly Type[] GENERATED_SHADER_CTOR_ARG_TYPES = { typeof(GraphicsResources), typeof(ShaderParameters?) };

    private ICBShader CreateGeneratedShader<TShader>(Type vertexType, ShaderParameters? shaderParameterOverrides)
            where TShader : IAutoShader {
        Debug.Assert(GetVertexTypeFromVS<TShader>() == vertexType,
            "Tried getting shader from cache with incorret vertex type.");

        var parameterizedType = typeof(GeneratedShader<,,,,,>)
            .MakeGenericType(typeof(TShader),
                vertexType,
                TShader.VertexBlockType,
                TShader.FragmentBlockType,
                TShader.InstanceVertexBlockType,
                TShader.InstanceFragmentBlockType
            );
        var ctor = parameterizedType.GetConstructor(GENERATED_SHADER_CTOR_ARG_TYPES);
        Debug.Assert(ctor != null, "Could not find required ctor on GeneratedShader!?");
        return (ICBShader)ctor.Invoke(new object?[] { _gfxRes, shaderParameterOverrides });
    }

    private Type GetVertexTypeFromVS<TShader>()
        => typeof(TShader).GetMethods()
            .Single(x => x.GetCustomAttribute<VertexShaderAttribute>() != null)
            .GetParameters()
            .Single().ParameterType;

    public IEnumerable<ICBShader> ActiveShaders => _dic.Select(x => x.Value);
}
