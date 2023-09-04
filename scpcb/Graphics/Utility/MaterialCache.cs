using scpcb.Graphics.Primitives;
using scpcb.Graphics.Shaders.Utility;
using Veldrid;

namespace scpcb.Graphics.Utility; 

public class MaterialCache : BaseCache<int, ICBMaterial> {
    private readonly GraphicsDevice _gfx;
    private readonly ShaderCache _shaderCache;

    public MaterialCache(GraphicsDevice gfx, ShaderCache shaderCache) {
        _gfx = gfx;
        _shaderCache = shaderCache;
    }

    private static int GetHashCode(ICBShader shader, IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers) {
        var hashCode = new HashCode();
        hashCode.Add(shader);
        foreach (var t in textures) {
            hashCode.Add(t);
        }
        foreach (var s in samplers) {
            hashCode.Add(s);
        }
        return hashCode.ToHashCode();
    }
    
    public ICBMaterial<TVertex> GetMaterial<TShader, TVertex>(IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers)
            where TShader : IAutoShader
        => GetMaterial(_shaderCache.GetShader<TShader, TVertex>(), textures, samplers);

    public ICBMaterial<TVertex> GetMaterial<TVertex>(ICBShader<TVertex> shader, IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers)
        => (ICBMaterial<TVertex>)GetMaterial((ICBShader)shader, textures, samplers);

    public ICBMaterial GetMaterial<TShader>(IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers)
            where TShader : IAutoShader
        => GetMaterial(_shaderCache.GetShader<TShader>(), textures, samplers);

    public ICBMaterial GetMaterial(ICBShader shader, IEnumerable<ICBTexture> textures, IEnumerable<Sampler> samplers) {
        var hash = GetHashCode(shader, textures, samplers);
        return _dic.TryGetValue(hash, out var val)
            ? val
            : _dic[hash] = shader.CreateMaterial(textures, samplers);
    }
}
